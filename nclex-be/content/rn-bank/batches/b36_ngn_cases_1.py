"""Batch 36: NGN unfolding case studies I - diabetic ketoacidosis; acute ischemic stroke with alteplase.

Each case has six items that follow the clinical judgment measurement model in order:
recognize cues, analyze cues, prioritize hypotheses, generate solutions, take action, evaluate outcomes.
"""
from qb import *

E = "endocrine"
N = "neurological"
DKA = "diabetic-ketoacidosis"
ST = "stroke-care"

# ---- Case 1: diabetic ketoacidosis -------------------------------------------------------------
dka_tabs = [
    ("notes", "Nurses' notes", [
        note("Emergency department, 0800",
             "A 19-year-old with type 1 diabetes has had vomiting and abdominal pain for 24 hours. The client stopped taking insulin yesterday \"because I wasn't eating.\" Breathing is deep and rapid, with a fruity odor. Mucous membranes are dry. The client is drowsy but oriented.", id="note-0800"),
        note("1200",
             "The client is harder to rouse than at 0800 and reports a new headache.", id="note-1200")]),
    ("vitals", "Vital signs", [
        flow("Vital signs", ["0800", "1000", "1200"], [
            ("Heart rate (/min)", ["128", "118", "104"]),
            ("Respiratory rate (/min)", ["32", "28", "22"]),
            ("Blood pressure (mm Hg)", ["96/58", "104/62", "112/70"]),
            ("Temperature (°C)", ["37.4", "37.3", "37.1"]),
            ("SpO2 (%)", ["98", "98", "99"])], id="vitals-flow")]),
    ("labs", "Laboratory results", [
        table("Laboratory results", ["0800", "1200"], [
            ("Glucose (mg/dL)", ["548", "240"]),
            ("Arterial pH", ["7.12", "7.26"]),
            ("Bicarbonate (mEq/L)", ["9", "15"]),
            ("Potassium (mEq/L)", ["5.4", "3.6"]),
            ("Anion gap", ["26", "16"]),
            ("Serum ketones", ["Large", "Moderate"])], id="labs-table")]),
]

dka_items = [
hl_table(M("PHI.PA", DKA, "recognize_cues", "moderate", E, "ms"),
   "Review the 0800 findings. Highlight the findings that require immediate follow-up.",
   ["Finding", "0800 result"],
   [(["Glucose", "548 mg/dL"], True, "Severe hyperglycemia from insulin deficiency."),
    (["Arterial pH", "7.12"], True, "Significant acidosis."),
    (["Bicarbonate", "9 mEq/L"], True, "Bicarbonate is consumed buffering ketoacids."),
    (["Respiratory rate", "32/min, deep"], True, "Kussmaul respirations compensate for acidosis."),
    (["SpO2", "98%"], False, "Oxygenation is adequate."),
    (["Temperature", "37.4 °C"], False, "There's no significant fever.")],
   "Hyperglycemia, acidosis and Kussmaul breathing are the key cues.",
   "Recognize key cues in DKA."),

cloze(M("PHI.PA", DKA, "analyze_cues", "moderate", E, "ms"),
   "Complete the sentence.",
   "The client's findings are most consistent with {{b1}}, most likely caused by {{b2}}.",
   [[("diabetic ketoacidosis", True, "High glucose, low pH and bicarbonate, a wide anion gap and ketones indicate DKA."),
     ("hyperosmolar hyperglycemic state", False, "HHS has little ketosis and a near-normal pH."),
     ("lactic acidosis from sepsis", False, "There's no sign of infection, and ketones are large.")],
    [("stopping insulin while ill", True, "Insulin omission is a common trigger of DKA."),
     ("eating too much carbohydrate", False, "The client has been vomiting and not eating."),
     ("an insulin overdose", False, "This would cause hypoglycemia.")]],
   "Insulin deficiency drives fat breakdown and ketoacid production.",
   "Analyze the cause of DKA."),

mc(M("PHI.PA", DKA, "prioritize_hypotheses", "difficult", E, "ms"),
   "An IV insulin infusion is about to start. Which complication is the client at highest risk for during treatment?",
   [("Hypokalemia", True, "Insulin moves potassium into cells; total body potassium is already low despite a normal-looking level."),
    ("Hypernatremia", False, "This isn't the main risk."),
    ("Hypercalcemia", False, "This isn't related."),
    ("Fluid overload", False, "The client is dehydrated, although fluids are monitored.")],
   "Monitor potassium closely and replace it before and during insulin therapy.",
   "Anticipate hypokalemia during DKA treatment."),

matrix_mc(M("PHI.PA", DKA, "generate_solutions", "moderate", E, "ms"),
   "For each intervention, indicate whether it is indicated or not indicated for this client now.",
   ["Indicated", "Not indicated"],
   [("IV 0.9% sodium chloride bolus", 0, ["Restoring volume is the first priority.", "Fluids are essential."]),
    ("Continuous IV regular insulin infusion once the potassium is known to be 3.3 mEq/L or higher", 0, ["IV insulin stops ketone production.", "Insulin is essential."]),
    ("Hourly glucose checks", 0, ["Frequent checks guide the insulin infusion.", "Monitoring is essential."]),
    ("Sodium bicarbonate infusion", 1, ["Bicarbonate is reserved for very severe acidosis (pH below about 6.9).", "With a pH of 7.12, bicarbonate isn't routinely indicated."]),
    ("Subcutaneous insulin glargine as the only insulin", 1, ["Basal insulin alone can't treat acute DKA.", "IV insulin is needed now."]),
    ("Continuous cardiac monitoring", 0, ["Potassium shifts can cause dysrhythmias.", "Monitoring is indicated."])],
   "DKA bundle: fluids, potassium, IV insulin and close monitoring.",
   "Select interventions for DKA."),

order(M("PHI.PA", DKA, "take_action", "difficult", E, "ms"),
   "Place the steps of the client's DKA treatment in the order they should occur.",
   [("Start the IV 0.9% sodium chloride bolus", "Volume comes first."),
    ("Confirm the potassium level and replace it if needed", "Potassium must be 3.3 mEq/L or higher before insulin."),
    ("Start the IV regular insulin infusion", "Insulin stops ketogenesis."),
    ("Add dextrose to the IV fluids when the glucose falls to about 200 to 250 mg/dL", "This prevents hypoglycemia while the acidosis clears."),
    ("Give subcutaneous basal insulin 1 to 2 hours before stopping the infusion, once the anion gap closes", "Overlapping prevents rebound DKA.")],
   "Fluids, potassium, insulin, dextrose, then overlap to subcutaneous insulin.",
   "Sequence DKA treatment.", seed=36),

matrix_mc(M("PHI.PA", DKA, "evaluate_outcomes", "difficult", E, "ms"),
   "Review the 1200 findings. For each, indicate whether it shows improvement or needs immediate follow-up.",
   ["Improvement", "Needs immediate follow-up"],
   [("Glucose 240 mg/dL", 0, ["The glucose is falling steadily.", "This is an expected response."]),
    ("Arterial pH 7.26", 0, ["The acidosis is improving.", "This is an expected response."]),
    ("Heart rate 104/min", 0, ["The tachycardia is improving with fluids.", "This is improving."]),
    ("Harder to rouse, with a new headache", 1, ["Worsening mental status isn't improvement.", "This may signal cerebral edema, a dangerous complication; notify the provider right away."])],
   "Evaluate the trends, and recognize worsening neurologic status as an emergency.",
   "Evaluate the response to DKA treatment."),
]

# ---- Case 2: acute ischemic stroke ---------------------------------------------------------------
stroke_tabs = [
    ("ed-notes", "Emergency department notes", [
        note("Emergency department, 0950",
             "A 68-year-old with atrial fibrillation who doesn't take an anticoagulant was found by the spouse at 0930 with right arm weakness and garbled speech. Last known well: 0845. Right facial droop and right arm drift. The client understands questions but can't form words. Non-contrast head CT: no hemorrhage.", id="note-0950")]),
    ("stroke-vitals", "Vital signs", [
        flow("Vital signs", ["0950", "1010", "1030"], [
            ("Blood pressure (mm Hg)", ["192/108", "178/100", "172/96"]),
            ("Heart rate (/min)", ["98 irregular", "96 irregular", "94 irregular"]),
            ("Respiratory rate (/min)", ["18", "18", "16"]),
            ("SpO2 (%)", ["96", "97", "97"])], id="stroke-flow")]),
    ("stroke-labs", "Laboratory results", [
        table("Laboratory results", ["Result"], [
            ("Platelets", ["210,000/mm3"]),
            ("INR", ["1.0"]),
            ("Glucose", ["132 mg/dL"]),
            ("Creatinine", ["0.9 mg/dL"])], id="stroke-lab-table")]),
]

stroke_items = [
hl_text(M("PHI.PA", ST, "recognize_cues", "easy", N, "aha"),
   "Highlight the findings in the emergency department note that are consistent with an acute stroke.",
   ["The client has ",
    ("right facial droop", True, "Facial asymmetry is a stroke sign."),
    ", ",
    ("right arm drift", True, "Weakness on one side is a stroke sign."),
    " and ",
    ("can't form words despite understanding", True, "Expressive aphasia suggests a left hemisphere stroke."),
    ". The blood glucose is ",
    ("132 mg/dL", False, "This rules out hypoglycemia as a mimic; it isn't a stroke sign."),
    ", and the client has ",
    ("a history of atrial fibrillation", False, "This is a risk factor, not a current sign."),
    "."],
   "Sudden one-sided weakness, facial droop and speech problems are hallmark stroke signs.",
   "Recognize acute stroke findings."),

matrix_mc(M("PHI.PA", ST, "analyze_cues", "difficult", N, "aha"),
   "For each finding, indicate whether it supports giving alteplase now or must be addressed before alteplase.",
   ["Supports giving alteplase", "Must be addressed first"],
   [("Last known well at 0845", 0, ["The client is within the 3 to 4.5 hour window.", "Timing is acceptable."]),
    ("CT shows no hemorrhage", 0, ["Hemorrhage has been excluded.", "This is required and met."]),
    ("Blood pressure 192/108 mm Hg", 1, ["This is above the limit.", "BP must be below 185/110 before alteplase."]),
    ("Platelets 210,000/mm3", 0, ["The platelets are adequate.", "This isn't a barrier."]),
    ("INR 1.0", 0, ["Coagulation is normal.", "This isn't a barrier."])],
   "Eligibility: time window, no hemorrhage, BP below 185/110, adequate platelets and coagulation.",
   "Analyze eligibility for thrombolysis."),

cloze(M("PHI.PA", ST, "prioritize_hypotheses", "moderate", N, "aha"),
   "Complete the sentence.",
   "The nurse's priority before alteplase is to lower the {{b1}} to below {{b2}}.",
   [[("blood pressure", True, "High BP raises the risk of hemorrhage with thrombolysis."),
     ("blood glucose", False, "The glucose is acceptable."),
     ("heart rate", False, "The heart rate isn't the barrier.")],
    [("185/110 mm Hg", True, "This is the threshold before alteplase."),
     ("120/80 mm Hg", False, "Too aggressive lowering can extend the stroke."),
     ("140/90 mm Hg", False, "This isn't the required threshold.")]],
   "Treat BP to below 185/110 before alteplase and keep it below 180/105 afterward.",
   "Prioritize blood pressure before thrombolysis."),

sata(M("PHI.PA", ST, "generate_solutions", "moderate", N, "aha ms"),
   "The client receives alteplase. Which interventions should the nurse include for the next 24 hours?",
   [("Frequent neurological and vital sign checks per protocol", True, "Early detection of bleeding or worsening stroke."),
    ("Keep blood pressure below 180/105 mm Hg", True, "This reduces the risk of hemorrhage."),
    ("Avoid IM injections and other invasive procedures", True, "This reduces the risk of bleeding."),
    ("Keep the client NPO until a swallow screen is passed", True, "This prevents aspiration."),
    ("Give aspirin right away", False, "Antiplatelets are usually held for 24 hours after alteplase."),
    ("Insert a nasogastric tube right away", False, "Invasive tubes are delayed if possible.")],
   "Post-thrombolysis care: monitoring, BP control, bleeding precautions, swallow screening.",
   "Plan care after alteplase."),

mc(M("PHI.PA", ST, "take_action", "difficult", N, "aha"),
   "During the alteplase infusion, the client suddenly has a severe headache and vomits. What should the nurse do first?",
   [("Stop the alteplase infusion and notify the provider", True, "These signs suggest bleeding in the brain; a stat CT is needed."),
    ("Give an antiemetic and continue", False, "This ignores a possible hemorrhage."),
    ("Increase the infusion to finish faster", False, "This worsens bleeding."),
    ("Recheck in 30 minutes", False, "This needs immediate action.")],
   "A neurologic decline, headache or vomiting during thrombolysis means: stop the infusion, notify the provider and get a CT.",
   "Respond to suspected hemorrhage during thrombolysis."),

matrix_mc(M("PHI.PA", ST, "evaluate_outcomes", "moderate", N, "aha"),
   "On day 2, the nurse reviews new findings. For each, indicate whether it shows an expected or improving response or needs follow-up.",
   ["Expected or improving", "Needs follow-up"],
   [("Speech is more fluent", 0, ["Improving speech shows recovering function.", "This is improving."]),
    ("Right arm drift has decreased", 0, ["Strength is improving.", "This is improving."]),
    ("Blood pressure 168/94 mm Hg", 0, ["This is within the target (below 180/105).", "This meets the target."]),
    ("Black, tarry stool", 1, ["This isn't expected.", "Black stool suggests GI bleeding after thrombolysis."])],
   "Evaluate neurological recovery and watch for bleeding complications.",
   "Evaluate outcomes after stroke treatment."),
]

BUILT = build(36, "ngn-cases-1", "NGN case studies I: DKA and acute ischemic stroke", [], cases=[
    case("Diabetic ketoacidosis", dka_tabs, dka_items),
    case("Acute ischemic stroke", stroke_tabs, stroke_items),
])
