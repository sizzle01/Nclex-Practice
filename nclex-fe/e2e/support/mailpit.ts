// Reads the emails the worker sent to Mailpit (docker compose), through Mailpit's REST API.
const mailpitUrl = process.env.MAILPIT_URL ?? "http://localhost:8025";

type MessageSummary = { ID: string };
type Message = { Text: string };

/** The newest email to `to`, waiting up to `timeoutMs` for the worker's outbox to send it. */
async function latestMessageTo(to: string, timeoutMs = 30_000): Promise<Message> {
  const deadline = Date.now() + timeoutMs;
  while (Date.now() < deadline) {
    const search = await fetch(`${mailpitUrl}/api/v1/search?query=${encodeURIComponent(`to:"${to}"`)}`);
    const { messages } = (await search.json()) as { messages: MessageSummary[] };
    if (messages.length > 0) {
      const message = await fetch(`${mailpitUrl}/api/v1/message/${messages[0].ID}`);
      return (await message.json()) as Message;
    }
    await new Promise((resolve) => setTimeout(resolve, 500));
  }
  throw new Error(`No email to ${to} arrived within ${timeoutMs} ms`);
}

/** The path and query of the first link in the newest email to `to` that starts with `path`. */
export async function linkInEmail(to: string, path: string): Promise<string> {
  const { Text } = await latestMessageTo(to);
  const match = Text.match(new RegExp(`https?://[^\\s]+(${path}\\?[^\\s]+)`));
  if (!match) throw new Error(`The email to ${to} has no ${path} link`);
  return match[1];
}
