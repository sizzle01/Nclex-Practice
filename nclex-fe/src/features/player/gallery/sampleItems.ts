// The first question of each MVP type in nclex-be/content/sample-questions.rn.json, each with a
// partly wrong answer and the reveal the backend graders give for it, so every kind of mark shows.
import type { GalleryItem } from "./galleryTypes";

export const sampleItems: GalleryItem[] = [
  {
    itemType: "multiple_choice",
    stem: "The nurse receives report on four clients. Which client should the nurse see first?",
    content: {
      options: [
        {
          id: "a",
          text: "A client with COPD whose oxygen saturation is 90% on 2 L/min by nasal cannula",
        },
        {
          id: "b",
          text: "A client one day after an appendectomy who rates pain 4 out of 10",
        },
        {
          id: "c",
          text: "A client who received IV morphine 20 minutes ago and is now difficult to rouse with a respiratory rate of 8/min",
        },
        {
          id: "d",
          text: "A client with type 2 diabetes whose pre-lunch glucose is 180 mg/dL",
        },
      ],
    },
    revealResponse: {
      selected: "a",
    },
    reveal: {
      answerKey: {
        correct: "c",
      },
      grade: {
        points: 0,
        maxPoints: 1,
        isCorrect: false,
        detail: [
          {
            target: "a",
            delta: 0,
            selected: "a",
            missed: false,
          },
          {
            target: "b",
            delta: 0,
            missed: false,
          },
          {
            target: "c",
            delta: 0,
            missed: true,
          },
          {
            target: "d",
            delta: 0,
            missed: false,
          },
        ],
      },
      rationale: {
        correct:
          "Opioid-induced respiratory depression is an immediate threat to airway and breathing, so this client is the priority.",
        perOption: {
          a: "A saturation of 88% to 92% is an acceptable target for many clients with COPD; this client is stable.",
          b: "Moderate pain one day after surgery is expected and can be managed after the unstable client is seen.",
          c: "Sedation with a respiratory rate of 8/min after an opioid signals respiratory depression; the nurse must assess and support breathing now.",
          d: "Mild hyperglycemia before a meal needs follow-up but is not life-threatening.",
        },
        references: [
          "SAMPLE CONTENT: written for development and testing, not reviewed by a nurse educator. Do not use for exam preparation.",
        ],
      },
      educationalObjective: "Use airway, breathing and circulation to choose which client to see first.",
      tags: {
        clientNeeds: "SECE.MOC",
        bodySystem: "fundamentals",
        cjmmSkill: "prioritize_hypotheses",
        difficulty: "moderate",
      },
    },
  },
  {
    itemType: "sata",
    stem: "The nurse is assigning care for a stable client to assistive personnel. Which tasks are appropriate to delegate? Select all that apply.",
    content: {
      options: [
        {
          id: "a",
          text: "Measuring vital signs",
        },
        {
          id: "b",
          text: "Helping the client walk in the hall",
        },
        {
          id: "c",
          text: "Assessing the client on admission",
        },
        {
          id: "d",
          text: "Teaching the client to self-inject insulin",
        },
        {
          id: "e",
          text: "Recording intake and output",
        },
        {
          id: "f",
          text: "Evaluating the client's response to pain medication",
        },
      ],
    },
    revealResponse: {
      selected: ["a", "b", "c"],
    },
    reveal: {
      answerKey: {
        correct: ["a", "b", "e"],
      },
      grade: {
        points: 1,
        maxPoints: 3,
        isCorrect: false,
        detail: [
          {
            target: "a",
            delta: 1,
            selected: "a",
            missed: false,
          },
          {
            target: "b",
            delta: 1,
            selected: "b",
            missed: false,
          },
          {
            target: "c",
            delta: -1,
            selected: "c",
            missed: false,
          },
          {
            target: "d",
            delta: 0,
            missed: false,
          },
          {
            target: "e",
            delta: 0,
            missed: true,
          },
          {
            target: "f",
            delta: 0,
            missed: false,
          },
        ],
      },
      rationale: {
        correct:
          "Assistive personnel can do routine, standard tasks for stable clients; assessment, teaching and evaluation stay with the nurse.",
        perOption: {
          a: "Measuring vital signs on a stable client is a routine task.",
          b: "Helping a stable client walk is within the role of assistive personnel.",
          c: "Admission assessment requires nursing judgment and cannot be delegated.",
          d: "Teaching is a nursing responsibility.",
          e: "Measuring and recording intake and output is a routine task.",
          f: "Evaluating a response to medication requires nursing judgment.",
        },
        references: [
          "SAMPLE CONTENT: written for development and testing, not reviewed by a nurse educator. Do not use for exam preparation.",
        ],
      },
      educationalObjective: "Delegate tasks, not assessment, teaching or evaluation.",
      tags: {
        clientNeeds: "SECE.MOC",
        bodySystem: "fundamentals",
        cjmmSkill: "take_action",
        difficulty: "moderate",
      },
    },
  },
  {
    itemType: "select_n",
    stem: "The nurse is caring for a client 12 hours after a total thyroidectomy. Select the 3 findings that require immediate follow-up.",
    content: {
      options: [
        {
          id: "a",
          text: "Tingling around the mouth and in the fingertips",
        },
        {
          id: "b",
          text: "Mild hoarseness when speaking",
        },
        {
          id: "c",
          text: "High-pitched sound on inspiration",
        },
        {
          id: "d",
          text: "Incision pain rated 3 out of 10",
        },
        {
          id: "e",
          text: "Facial twitching when the cheek is tapped",
        },
        {
          id: "f",
          text: "Temperature 37.2 °C (99 °F)",
        },
      ],
      selectCount: 3,
    },
    revealResponse: {
      selected: ["a", "b", "c"],
    },
    reveal: {
      answerKey: {
        correct: ["a", "c", "e"],
      },
      grade: {
        points: 2,
        maxPoints: 3,
        isCorrect: false,
        detail: [
          {
            target: "a",
            delta: 1,
            selected: "a",
            missed: false,
          },
          {
            target: "b",
            delta: 0,
            selected: "b",
            missed: false,
          },
          {
            target: "c",
            delta: 1,
            selected: "c",
            missed: false,
          },
          {
            target: "d",
            delta: 0,
            missed: false,
          },
          {
            target: "e",
            delta: 0,
            missed: true,
          },
          {
            target: "f",
            delta: 0,
            missed: false,
          },
        ],
      },
      rationale: {
        correct:
          "Damage to the parathyroid glands causes hypocalcemia (tingling, a positive Chvostek sign), and stridor signals airway obstruction; all three need action now.",
        perOption: {
          a: "Perioral and fingertip tingling is an early sign of hypocalcemia from parathyroid injury.",
          b: "Mild hoarseness is common after thyroid surgery because of laryngeal edema and usually resolves.",
          c: "Stridor signals airway narrowing from edema, bleeding or tetany and is an emergency.",
          d: "Mild incision pain is expected and can be managed with the prescribed analgesic.",
          e: "Facial twitching on tapping (Chvostek sign) indicates hypocalcemia.",
          f: "A slightly raised temperature in the first 24 hours is expected after surgery.",
        },
        references: [
          "SAMPLE CONTENT: written for development and testing, not reviewed by a nurse educator. Do not use for exam preparation.",
        ],
      },
      educationalObjective: "After thyroidectomy, watch for airway obstruction and hypocalcemia.",
      tags: {
        clientNeeds: "PHI.PA",
        bodySystem: "endocrine",
        cjmmSkill: "recognize_cues",
        difficulty: "moderate",
      },
    },
  },
  {
    itemType: "matrix_mc",
    stem: "For each finding, indicate whether it is more consistent with left-sided or right-sided heart failure.",
    content: {
      rows: [
        {
          id: "r1",
          text: "Crackles in both lung bases",
        },
        {
          id: "r2",
          text: "Jugular venous distension",
        },
        {
          id: "r3",
          text: "Dependent edema of the ankles",
        },
        {
          id: "r4",
          text: "Orthopnea",
        },
      ],
      columns: [
        {
          id: "c1",
          text: "Left-sided",
        },
        {
          id: "c2",
          text: "Right-sided",
        },
      ],
    },
    revealResponse: {
      byRow: {
        r1: "c2",
        r2: "c2",
        r3: "c2",
        r4: "c1",
      },
    },
    reveal: {
      answerKey: {
        byRow: {
          r1: "c1",
          r2: "c2",
          r3: "c2",
          r4: "c1",
        },
      },
      grade: {
        points: 3,
        maxPoints: 4,
        isCorrect: false,
        detail: [
          {
            target: "r1",
            delta: 0,
            selected: "c2",
            expected: "c1",
            missed: true,
          },
          {
            target: "r2",
            delta: 1,
            selected: "c2",
            expected: "c2",
            missed: false,
          },
          {
            target: "r3",
            delta: 1,
            selected: "c2",
            expected: "c2",
            missed: false,
          },
          {
            target: "r4",
            delta: 1,
            selected: "c1",
            expected: "c1",
            missed: false,
          },
        ],
      },
      rationale: {
        correct:
          "Left-sided failure backs blood up into the lungs; right-sided failure backs blood up into the systemic veins.",
        perOption: {
          "r1:c1": "Crackles come from fluid in the lungs, typical of left-sided failure.",
          "r1:c2": "Right-sided failure congests the systemic veins, not the lungs.",
          "r2:c1": "Left-sided failure congests the pulmonary circulation, not the neck veins.",
          "r2:c2": "Jugular venous distension reflects systemic venous congestion from right-sided failure.",
          "r3:c1": "Peripheral edema is not the main sign of left-sided failure.",
          "r3:c2": "Dependent edema results from systemic venous congestion in right-sided failure.",
          "r4:c1": "Orthopnea occurs because lying flat increases pulmonary congestion in left-sided failure.",
          "r4:c2": "Breathlessness lying flat comes from lung congestion, which is left-sided.",
        },
        references: [
          "SAMPLE CONTENT: written for development and testing, not reviewed by a nurse educator. Do not use for exam preparation.",
        ],
      },
      educationalObjective: "Tell left-sided from right-sided heart failure by where fluid backs up.",
      tags: {
        clientNeeds: "PHI.PA",
        bodySystem: "cardiovascular",
        cjmmSkill: "analyze_cues",
        difficulty: "moderate",
      },
    },
  },
  {
    itemType: "matrix_mr",
    stem: "For each medication, select the adverse effects the nurse should teach the client to report right away. Each medication may have more than one.",
    content: {
      rows: [
        {
          id: "r1",
          text: "Warfarin",
        },
        {
          id: "r2",
          text: "Lisinopril",
        },
        {
          id: "r3",
          text: "Metformin",
        },
      ],
      columns: [
        {
          id: "c1",
          text: "Unusual bleeding or bruising",
        },
        {
          id: "c2",
          text: "Swelling of the face, lips or tongue",
        },
        {
          id: "c3",
          text: "Muscle pain with weakness and trouble breathing",
        },
        {
          id: "c4",
          text: "Black, tarry stools",
        },
      ],
      groupAxis: "row",
    },
    revealResponse: {
      byGroup: {
        r1: ["r1:c1", "r1:c2"],
        r2: ["r2:c2"],
        r3: ["r3:c3"],
      },
    },
    reveal: {
      answerKey: {
        byGroup: {
          r1: ["r1:c1", "r1:c4"],
          r2: ["r2:c2"],
          r3: ["r3:c3"],
        },
      },
      grade: {
        points: 2,
        maxPoints: 4,
        isCorrect: false,
        detail: [
          {
            target: "r1:c1",
            delta: 1,
            selected: "r1:c1",
            missed: false,
          },
          {
            target: "r1:c2",
            delta: -1,
            selected: "r1:c2",
            missed: false,
          },
          {
            target: "r1:c3",
            delta: 0,
            missed: false,
          },
          {
            target: "r1:c4",
            delta: 0,
            missed: true,
          },
          {
            target: "r2:c1",
            delta: 0,
            missed: false,
          },
          {
            target: "r2:c2",
            delta: 1,
            selected: "r2:c2",
            missed: false,
          },
          {
            target: "r2:c3",
            delta: 0,
            missed: false,
          },
          {
            target: "r2:c4",
            delta: 0,
            missed: false,
          },
          {
            target: "r3:c1",
            delta: 0,
            missed: false,
          },
          {
            target: "r3:c2",
            delta: 0,
            missed: false,
          },
          {
            target: "r3:c3",
            delta: 1,
            selected: "r3:c3",
            missed: false,
          },
          {
            target: "r3:c4",
            delta: 0,
            missed: false,
          },
        ],
      },
      rationale: {
        correct:
          "Teaching focuses on the serious adverse effect of each drug: bleeding for warfarin, angioedema for lisinopril, lactic acidosis for metformin.",
        perOption: {
          "r1:c1": "Bleeding is the main adverse effect of warfarin.",
          "r1:c2": "Angioedema is linked to ACE inhibitors, not warfarin.",
          "r1:c3": "This describes lactic acidosis, a metformin effect.",
          "r1:c4": "Tarry stools can signal gastrointestinal bleeding on warfarin.",
          "r2:c1": "Lisinopril does not cause bleeding.",
          "r2:c2": "ACE inhibitors can cause angioedema, which can block the airway.",
          "r2:c3": "This is not an effect of lisinopril.",
          "r2:c4": "Lisinopril does not cause gastrointestinal bleeding.",
          "r3:c1": "Metformin does not cause bleeding.",
          "r3:c2": "Metformin does not cause angioedema.",
          "r3:c3": "Muscle pain, weakness and trouble breathing can signal lactic acidosis from metformin.",
          "r3:c4": "Metformin does not cause gastrointestinal bleeding.",
        },
        references: [
          "SAMPLE CONTENT: written for development and testing, not reviewed by a nurse educator. Do not use for exam preparation.",
        ],
      },
      educationalObjective: "Teach clients the serious adverse effects of common medications.",
      tags: {
        clientNeeds: "PHI.PPT",
        bodySystem: "pharmacology",
        cjmmSkill: "take_action",
        difficulty: "moderate",
      },
    },
  },
  {
    itemType: "multiple_response_grouping",
    stem: "The nurse assesses a client 1 day after a total hip arthroplasty. For each body system, select the findings that require immediate follow-up.",
    content: {
      groups: [
        {
          id: "g1",
          label: "Circulation",
          options: [
            {
              id: "a",
              text: "Capillary refill under 3 seconds in the toes",
            },
            {
              id: "b",
              text: "Cool, pale foot on the operative side",
            },
            {
              id: "c",
              text: "Pedal pulses 2+ and equal",
            },
          ],
        },
        {
          id: "g2",
          label: "Respiratory",
          options: [
            {
              id: "d",
              text: "Sudden shortness of breath and chest pain",
            },
            {
              id: "e",
              text: "Oxygen saturation 96% on room air",
            },
            {
              id: "f",
              text: "Clear breath sounds",
            },
          ],
        },
        {
          id: "g3",
          label: "Operative hip",
          options: [
            {
              id: "g",
              text: "Bright red drainage saturating the dressing",
            },
            {
              id: "h",
              text: "Operative leg shorter and turned outward",
            },
            {
              id: "i",
              text: "Mild bruising around the incision",
            },
          ],
        },
      ],
    },
    revealResponse: {
      byGroup: {
        g1: ["b", "a"],
        g2: ["d"],
        g3: ["g", "h"],
      },
    },
    reveal: {
      answerKey: {
        byGroup: {
          g1: ["b"],
          g2: ["d"],
          g3: ["g", "h"],
        },
      },
      grade: {
        points: 3,
        maxPoints: 4,
        isCorrect: false,
        detail: [
          {
            target: "a",
            delta: -1,
            selected: "a",
            missed: false,
          },
          {
            target: "b",
            delta: 1,
            selected: "b",
            missed: false,
          },
          {
            target: "c",
            delta: 0,
            missed: false,
          },
          {
            target: "d",
            delta: 1,
            selected: "d",
            missed: false,
          },
          {
            target: "e",
            delta: 0,
            missed: false,
          },
          {
            target: "f",
            delta: 0,
            missed: false,
          },
          {
            target: "g",
            delta: 1,
            selected: "g",
            missed: false,
          },
          {
            target: "h",
            delta: 1,
            selected: "h",
            missed: false,
          },
          {
            target: "i",
            delta: 0,
            missed: false,
          },
        ],
      },
      rationale: {
        correct:
          "After hip arthroplasty, watch for impaired circulation, pulmonary embolism, hemorrhage and dislocation.",
        perOption: {
          a: "Brisk capillary refill shows adequate perfusion.",
          b: "A cool, pale foot suggests impaired arterial circulation.",
          c: "Strong, equal pulses are a normal finding.",
          d: "Sudden dyspnea and chest pain suggest a pulmonary embolism.",
          e: "A saturation of 96% is within normal limits.",
          f: "Clear breath sounds are a normal finding.",
          g: "Saturating the dressing with bright red blood suggests hemorrhage.",
          h: "Shortening and external rotation suggest hip dislocation.",
          i: "Mild bruising is expected after surgery.",
        },
        references: [
          "SAMPLE CONTENT: written for development and testing, not reviewed by a nurse educator. Do not use for exam preparation.",
        ],
      },
      educationalObjective: "Recognize the complications of total hip arthroplasty.",
      tags: {
        clientNeeds: "PHI.RRP",
        bodySystem: "musculoskeletal",
        cjmmSkill: "recognize_cues",
        difficulty: "moderate",
      },
    },
  },
  {
    itemType: "dropdown_cloze",
    stem: "A client with type 1 diabetes is shaky, sweaty and anxious 2 hours after taking insulin and skipping lunch. Complete the sentence.",
    content: {
      template: "The client is most likely experiencing {{b1}}, as shown by {{b2}}.",
      blanks: [
        {
          id: "b1",
          options: [
            {
              id: "a",
              text: "hypoglycemia",
            },
            {
              id: "b",
              text: "hyperglycemia",
            },
            {
              id: "c",
              text: "diabetic ketoacidosis",
            },
          ],
        },
        {
          id: "b2",
          options: [
            {
              id: "a",
              text: "a capillary glucose of 52 mg/dL",
            },
            {
              id: "b",
              text: "a fruity odor on the breath",
            },
            {
              id: "c",
              text: "frequent urination",
            },
          ],
        },
      ],
    },
    revealResponse: {
      byBlank: {
        b1: "b",
        b2: "a",
      },
    },
    reveal: {
      answerKey: {
        byBlank: {
          b1: "a",
          b2: "a",
        },
      },
      grade: {
        points: 1,
        maxPoints: 2,
        isCorrect: false,
        detail: [
          {
            target: "b1",
            delta: 0,
            selected: "b",
            expected: "a",
            missed: true,
          },
          {
            target: "b2",
            delta: 1,
            selected: "a",
            expected: "a",
            missed: false,
          },
        ],
      },
      rationale: {
        correct:
          "Insulin without food leads to low glucose; shakiness and sweating are adrenergic signs, confirmed by a glucose below 70 mg/dL.",
        perOption: {
          "b1:a": "Insulin with a missed meal and adrenergic symptoms point to hypoglycemia.",
          "b1:b": "Hyperglycemia causes thirst and polyuria, not shakiness and sweating.",
          "b1:c": "DKA develops over hours with ketosis; it does not follow a missed meal after insulin.",
          "b2:a": "A glucose of 52 mg/dL confirms hypoglycemia.",
          "b2:b": "A fruity breath odor suggests ketosis from DKA.",
          "b2:c": "Frequent urination is a sign of hyperglycemia.",
        },
        references: [
          "SAMPLE CONTENT: written for development and testing, not reviewed by a nurse educator. Do not use for exam preparation.",
        ],
      },
      educationalObjective: "Recognize hypoglycemia and its confirming finding.",
      tags: {
        clientNeeds: "PHI.RRP",
        bodySystem: "endocrine",
        cjmmSkill: "analyze_cues",
        difficulty: "easy",
      },
    },
  },
  {
    itemType: "dropdown_table",
    stem: "The nurse reviews the morning laboratory results of a client with chronic kidney disease. For each result, choose the interpretation.",
    content: {
      columns: ["Laboratory test", "Result", "Interpretation"],
      rows: [
        {
          id: "r1",
          cells: [
            {
              text: "Potassium",
            },
            {
              text: "5.9 mEq/L",
            },
            {
              blankId: "b1",
            },
          ],
        },
        {
          id: "r2",
          cells: [
            {
              text: "Sodium",
            },
            {
              text: "138 mEq/L",
            },
            {
              blankId: "b2",
            },
          ],
        },
        {
          id: "r3",
          cells: [
            {
              text: "Fasting glucose",
            },
            {
              text: "62 mg/dL",
            },
            {
              blankId: "b3",
            },
          ],
        },
      ],
      blanks: [
        {
          id: "b1",
          options: [
            {
              id: "a",
              text: "Low",
            },
            {
              id: "b",
              text: "Within normal range",
            },
            {
              id: "c",
              text: "High",
            },
          ],
        },
        {
          id: "b2",
          options: [
            {
              id: "a",
              text: "Low",
            },
            {
              id: "b",
              text: "Within normal range",
            },
            {
              id: "c",
              text: "High",
            },
          ],
        },
        {
          id: "b3",
          options: [
            {
              id: "a",
              text: "Low",
            },
            {
              id: "b",
              text: "Within normal range",
            },
            {
              id: "c",
              text: "High",
            },
          ],
        },
      ],
    },
    revealResponse: {
      byBlank: {
        b1: "a",
        b2: "b",
        b3: "a",
      },
    },
    reveal: {
      answerKey: {
        byBlank: {
          b1: "c",
          b2: "b",
          b3: "a",
        },
      },
      grade: {
        points: 2,
        maxPoints: 3,
        isCorrect: false,
        detail: [
          {
            target: "b1",
            delta: 0,
            selected: "a",
            expected: "c",
            missed: true,
          },
          {
            target: "b2",
            delta: 1,
            selected: "b",
            expected: "b",
            missed: false,
          },
          {
            target: "b3",
            delta: 1,
            selected: "a",
            expected: "a",
            missed: false,
          },
        ],
      },
      rationale: {
        correct: "Normal ranges: potassium 3.5 to 5.0 mEq/L, sodium 135 to 145 mEq/L, fasting glucose 70 to 99 mg/dL.",
        perOption: {
          "b1:a": "5.9 mEq/L is above, not below, 3.5 to 5.0 mEq/L.",
          "b1:b": "5.9 mEq/L is above the normal 3.5 to 5.0 mEq/L.",
          "b1:c": "5.9 mEq/L is high; kidney disease reduces potassium excretion.",
          "b2:a": "138 mEq/L is not below 135 mEq/L.",
          "b2:b": "138 mEq/L is within 135 to 145 mEq/L.",
          "b2:c": "138 mEq/L is not above 145 mEq/L.",
          "b3:a": "62 mg/dL is below the fasting range of 70 to 99 mg/dL.",
          "b3:b": "62 mg/dL is below the normal fasting range.",
          "b3:c": "62 mg/dL is low, not high.",
        },
        references: [
          "SAMPLE CONTENT: written for development and testing, not reviewed by a nurse educator. Do not use for exam preparation.",
        ],
      },
      educationalObjective: "Interpret common electrolyte and glucose results.",
      tags: {
        clientNeeds: "PHI.RRP",
        bodySystem: "renal-urinary",
        cjmmSkill: "recognize_cues",
        difficulty: "moderate",
      },
    },
  },
  {
    itemType: "highlight_text",
    stem: "Highlight the findings in the nurse's note that require immediate follow-up.",
    content: {
      segments: [
        {
          id: "s1",
          text: "Client is 2 hours after a vaginal birth. ",
          selectable: false,
        },
        {
          id: "s2",
          text: "Fundus is boggy and 2 cm above the umbilicus",
          selectable: true,
        },
        {
          id: "s3",
          text: ". ",
          selectable: false,
        },
        {
          id: "s4",
          text: "Lochia rubra is saturating a pad every 15 minutes",
          selectable: true,
        },
        {
          id: "s5",
          text: ". ",
          selectable: false,
        },
        {
          id: "s6",
          text: "Blood pressure 118/72 mm Hg",
          selectable: true,
        },
        {
          id: "s7",
          text: ". ",
          selectable: false,
        },
        {
          id: "s8",
          text: "Client reports cramping while breastfeeding",
          selectable: true,
        },
        {
          id: "s9",
          text: ".",
          selectable: false,
        },
      ],
    },
    revealResponse: {
      selected: ["s2", "s6"],
    },
    reveal: {
      answerKey: {
        correct: ["s2", "s4"],
      },
      grade: {
        points: 0,
        maxPoints: 2,
        isCorrect: false,
        detail: [
          {
            target: "s2",
            delta: 1,
            selected: "s2",
            missed: false,
          },
          {
            target: "s4",
            delta: 0,
            missed: true,
          },
          {
            target: "s6",
            delta: -1,
            selected: "s6",
            missed: false,
          },
          {
            target: "s8",
            delta: 0,
            missed: false,
          },
        ],
      },
      rationale: {
        correct: "A boggy, high fundus with heavy bleeding indicates uterine atony and postpartum hemorrhage.",
        perOption: {
          s2: "A boggy fundus above the umbilicus suggests uterine atony.",
          s4: "Saturating a pad in 15 minutes is excessive bleeding.",
          s6: "This blood pressure is within normal limits.",
          s8: "Cramping while breastfeeding is expected as oxytocin contracts the uterus.",
        },
        references: [
          "SAMPLE CONTENT: written for development and testing, not reviewed by a nurse educator. Do not use for exam preparation.",
        ],
      },
      educationalObjective: "Recognize the early signs of postpartum hemorrhage.",
      tags: {
        clientNeeds: "PHI.PA",
        bodySystem: "maternal-newborn",
        cjmmSkill: "recognize_cues",
        difficulty: "moderate",
      },
    },
  },
  {
    itemType: "highlight_table",
    stem: "A client is receiving a unit of packed red blood cells. Highlight the entries that indicate a transfusion reaction.",
    content: {
      columns: ["Time", "Finding"],
      rows: [
        {
          id: "r1",
          cells: ["Before transfusion", "T 36.8 °C, HR 82/min, BP 124/78 mm Hg"],
          selectable: true,
        },
        {
          id: "r2",
          cells: ["15 minutes", "T 36.9 °C, HR 84/min, BP 122/76 mm Hg"],
          selectable: true,
        },
        {
          id: "r3",
          cells: ["30 minutes", "T 38.4 °C, HR 112/min, BP 98/60 mm Hg"],
          selectable: true,
        },
        {
          id: "r4",
          cells: ["30 minutes", "Client reports chills and low back pain"],
          selectable: true,
        },
        {
          id: "r5",
          cells: ["35 minutes", "Transfusion stopped; provider notified"],
          selectable: false,
        },
      ],
    },
    revealResponse: {
      selected: ["r1", "r3"],
    },
    reveal: {
      answerKey: {
        correct: ["r3", "r4"],
      },
      grade: {
        points: 0,
        maxPoints: 2,
        isCorrect: false,
        detail: [
          {
            target: "r1",
            delta: -1,
            selected: "r1",
            missed: false,
          },
          {
            target: "r2",
            delta: 0,
            missed: false,
          },
          {
            target: "r3",
            delta: 1,
            selected: "r3",
            missed: false,
          },
          {
            target: "r4",
            delta: 0,
            missed: true,
          },
        ],
      },
      rationale: {
        correct:
          "Fever, tachycardia, hypotension, chills and back pain during a transfusion indicate an acute hemolytic reaction.",
        perOption: {
          r1: "Baseline vital signs are normal.",
          r2: "Vital signs at 15 minutes are stable.",
          r3: "Fever, tachycardia and falling blood pressure signal a reaction.",
          r4: "Chills and low back pain are classic signs of a hemolytic reaction.",
        },
        references: [
          "SAMPLE CONTENT: written for development and testing, not reviewed by a nurse educator. Do not use for exam preparation.",
        ],
      },
      educationalObjective: "Recognize an acute hemolytic transfusion reaction.",
      tags: {
        clientNeeds: "PHI.PPT",
        bodySystem: "hematologic-oncologic",
        cjmmSkill: "recognize_cues",
        difficulty: "moderate",
      },
    },
  },
  {
    itemType: "fill_blank_calc",
    stem: "The provider prescribes 1,000 mL of 0.9% sodium chloride to infuse over 8 hours. At what rate should the nurse set the infusion pump?",
    content: {
      unit: "mL/hr",
      decimals: 0,
      inputLabel: "Infusion rate",
    },
    revealResponse: {
      value: "120",
    },
    reveal: {
      answerKey: {
        value: 125,
        rounding: "exact",
      },
      grade: {
        points: 0,
        maxPoints: 1,
        isCorrect: false,
        detail: [
          {
            target: "value",
            delta: 0,
            selected: "120",
            expected: 125,
            missed: true,
          },
        ],
      },
      rationale: {
        correct: "Rate = volume ÷ time = 1,000 mL ÷ 8 hours = 125 mL/hr.",
        perOption: {},
        references: [
          "SAMPLE CONTENT: written for development and testing, not reviewed by a nurse educator. Do not use for exam preparation.",
        ],
      },
      educationalObjective: "Calculate an infusion rate in mL/hr.",
      tags: {
        clientNeeds: "PHI.PPT",
        bodySystem: "pharmacology",
        cjmmSkill: "take_action",
        difficulty: "easy",
      },
    },
  },
  {
    itemType: "ordered_response",
    stem: "Place the steps for giving eye drops in order.",
    content: {
      items: [
        {
          id: "c",
          text: "Pull the lower lid down to form a pocket",
        },
        {
          id: "a",
          text: "Perform hand hygiene and put on gloves",
        },
        {
          id: "e",
          text: "Press gently on the inner corner of the eye",
        },
        {
          id: "b",
          text: "Ask the client to tilt the head back and look up",
        },
        {
          id: "d",
          text: "Place the drops in the pocket of the lower lid",
        },
      ],
    },
    revealResponse: {
      order: ["b", "a", "c", "d", "e"],
    },
    reveal: {
      answerKey: {
        order: ["a", "b", "c", "d", "e"],
        scoring: "all_or_nothing",
      },
      grade: {
        points: 0,
        maxPoints: 1,
        isCorrect: false,
        detail: [
          {
            target: "a",
            delta: 0,
            selected: "b",
            expected: "a",
            missed: true,
          },
          {
            target: "b",
            delta: 0,
            selected: "a",
            expected: "b",
            missed: true,
          },
          {
            target: "c",
            delta: 1,
            selected: "c",
            expected: "c",
            missed: false,
          },
          {
            target: "d",
            delta: 1,
            selected: "d",
            expected: "d",
            missed: false,
          },
          {
            target: "e",
            delta: 1,
            selected: "e",
            expected: "e",
            missed: false,
          },
        ],
      },
      rationale: {
        correct:
          "Clean hands, position the client, make a pocket, instill the drops, then press on the inner canthus to limit systemic absorption.",
        perOption: {
          a: "Hand hygiene comes first to prevent infection.",
          b: "Looking up moves the cornea away from the dropper.",
          c: "The pocket receives the drops instead of the sensitive cornea.",
          d: "Drops go into the conjunctival sac, not onto the cornea.",
          e: "Pressure on the inner canthus reduces drainage into the nasolacrimal duct.",
        },
        references: [
          "SAMPLE CONTENT: written for development and testing, not reviewed by a nurse educator. Do not use for exam preparation.",
        ],
      },
      educationalObjective: "Give eye drops in the correct sequence.",
      tags: {
        clientNeeds: "PHI.PPT",
        bodySystem: "pharmacology",
        cjmmSkill: "take_action",
        difficulty: "easy",
      },
    },
  },
];
