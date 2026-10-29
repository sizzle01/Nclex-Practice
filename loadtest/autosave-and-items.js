// Load test (F16): 500 concurrent candidates autosaving answers and fetching items, the two calls
// the player makes most (architecture.md section 7: API p95 under 150 ms).
//
// Run against the compose app stack (docs/operations.md, "Load test"):
//   docker run --rm -i --add-host=host.docker.internal:host-gateway \
//     -e API_URL=http://host.docker.internal:8080 -v "$PWD/loadtest:/scripts" \
//     grafana/k6:1.3.0 run /scripts/autosave-and-items.js
import http from "k6/http";
import { check, sleep } from "k6";

const api = `${__ENV.API_URL ?? "http://localhost:8080"}/api/v1`;
const candidates = Number(__ENV.CANDIDATES ?? 500);
// "RN Sample Set" from the sample content: a sample set, so unverified accounts may take it.
const sampleSetId = "f06a0000-0000-4000-8000-000000000001";
// Its first two items are multiple choice with options a-d, so { selected } is a valid answer.
const answerablePositions = [1, 2];

export const options = {
  setupTimeout: "15m",
  scenarios: {
    candidates: {
      executor: "ramping-vus",
      stages: [
        { duration: "1m", target: candidates },
        { duration: "3m", target: candidates },
        { duration: "30s", target: 0 },
      ],
    },
  },
  thresholds: {
    "http_req_duration{name:autosave}": ["p(95)<150"],
    "http_req_duration{name:item}": ["p(95)<150"],
    "checks": ["rate>0.99"],
  },
};

const json = (token, clientId) => ({
  headers: {
    "Content-Type": "application/json",
    Authorization: `Bearer ${token}`,
    ...(clientId ? { "X-Attempt-Client": clientId } : {}),
  },
});

// One account per virtual user, created before the test starts. Sign-up is slow on purpose
// (argon2id), so this is done in batches and not measured.
export function setup() {
  const tokens = [];
  const run = Date.now();
  for (let start = 0; start < candidates; start += 25) {
    const batch = [];
    for (let i = start; i < Math.min(start + 25, candidates); i++) {
      batch.push(["POST", `${api}/auth/signup`, JSON.stringify({
        email: `load-${run}-${i}@example.test`,
        password: "correct-horse-battery",
        track: "RN",
      }), { headers: { "Content-Type": "application/json" } }]);
    }
    for (const response of http.batch(batch)) {
      if (response.status !== 201 && response.status !== 200) throw new Error(`signup failed: ${response.status} ${response.body}`);
      tokens.push(response.json("accessToken"));
    }
  }
  return { tokens };
}

// Per virtual user: its attempt and the next clientSeq, kept across iterations.
let attemptId = null;
let clientSeq = 0;
const clientId = `k6-${__VU}`;

export default function ({ tokens }) {
  const token = tokens[(__VU - 1) % tokens.length];
  if (attemptId === null) {
    const started = http.post(`${api}/attempts`, JSON.stringify({ testSetId: sampleSetId }), json(token));
    check(started, { "attempt started": (r) => r.status === 201 || r.status === 200 });
    attemptId = started.json("id");
    http.post(`${api}/attempts/${attemptId}/open`, null, json(token, clientId));
  }

  const position = answerablePositions[clientSeq % answerablePositions.length];
  clientSeq += 1;

  const item = http.get(`${api}/attempts/${attemptId}/items/${position}`, { ...json(token), tags: { name: "item" } });
  check(item, { "item loaded": (r) => r.status === 200 });

  const saved = http.put(
    `${api}/attempts/${attemptId}/items/${position}/response`,
    JSON.stringify({ response: { selected: clientSeq % 2 ? "a" : "b" }, clientSeq }),
    { ...json(token, clientId), tags: { name: "autosave" } },
  );
  check(saved, { "answer saved": (r) => r.status === 200 });

  // A candidate reads and answers; about one save and one item fetch per second each is generous.
  sleep(1);
}
