"""Batch 37: NGN unfolding case studies II - upper GI bleed; COPD exacerbation."""
from qb import *

GI = "gastrointestinal"
R = "respiratory"
UP = "upper-gi-disorders-and-bleeding"
AC = "asthma-and-copd-management"
LEVEL = ["Low", "Within normal range", "High"]

def level(correct, why):
    return [(t, t == correct, why if t == correct else f"The result is {correct.lower()}, not {t.lower()}.") for t in LEVEL]

# ---- Case 1: upper GI bleed --------------------------------------------------------------------
gi_tabs = [
    ("gi-notes", "Nurses' notes", [
        note("Admission, 0600",
             "A 58-year-old has taken ibuprofen 800 mg three times a day for back pain for 2 months. The client vomited dark, coffee-ground material twice overnight and has had black, tarry stools. Reports dizziness when standing. Skin is pale and cool. Temperature 36.8 °C; SpO2 97% on room air.", id="gi-note-0600"),
        note("0900",
             "After 2 units of packed red blood cells, the client feels less dizzy. At 0855 the client vomited 300 mL of coffee-ground material.", id="gi-note-0900")]),
    ("gi-vitals", "Vital signs", [
        flow("Vital signs", ["0600", "0700", "0900"], [
            ("Heart rate (/min)", ["112", "124", "98"]),
            ("Blood pressure (mm Hg)", ["98/60", "86/52", "112/68"]),
            ("Respiratory rate (/min)", ["22", "24", "18"])], id="gi-flow")]),
    ("gi-labs", "Laboratory results", [
        table("Laboratory results", ["0600", "0900"], [
            ("Hemoglobin (g/dL)", ["7.1", "9.0"]),
            ("BUN (mg/dL)", ["42", "38"]),
            ("Creatinine (mg/dL)", ["0.9", "0.9"]),
            ("INR", ["1.1", "1.1"])], id="gi-lab-table")]),
]

gi_items = [
hl_table(M("PHI.PA", UP, "recognize_cues", "moderate", GI, "ms"),
   "Highlight the findings that indicate blood loss or hypovolemia.",
   ["Finding", "Result"],
   [(["Heart rate at 0700", "124/min"], True, "Rising tachycardia shows compensation for volume loss."),
    (["Blood pressure at 0700", "86/52 mm Hg"], True, "Hypotension shows significant volume loss."),
    (["Hemoglobin at 0600", "7.1 g/dL"], True, "A low hemoglobin reflects blood loss."),
    (["Dizziness on standing", "Present"], True, "Orthostatic symptoms reflect hypovolemia."),
    (["Temperature", "36.8 °C"], False, "This is normal."),
    (["SpO2", "97%"], False, "Oxygenation is adequate.")],
   "Tachycardia, hypotension, orthostasis and a falling hemoglobin indicate hemorrhage.",
   "Recognize cues of GI hemorrhage."),

matrix_mc(M("PHI.PA", UP, "analyze_cues", "moderate", GI, "ms"),
   "For each finding, indicate whether it supports an upper GI bleed.",
   ["Supports an upper GI bleed", "Does not support an upper GI bleed"],
   [("Coffee-ground vomit", 0, ["Blood partly digested by stomach acid looks like coffee grounds.", "This strongly supports an upper source."]),
    ("Black, tarry stools", 0, ["Melena comes from digested blood from the upper GI tract.", "This supports an upper source."]),
    ("Long-term high-dose ibuprofen use", 0, ["NSAIDs cause peptic ulcers.", "This is a major risk factor."]),
    ("High BUN with a normal creatinine", 0, ["Digested blood protein raises the BUN.", "This pattern supports an upper GI bleed."]),
    ("Normal INR", 1, ["A normal INR doesn't point to an upper GI bleed.", "This rules out coagulopathy as a cause but doesn't support the diagnosis."])],
   "Coffee-ground emesis, melena, NSAID use and a high BUN-to-creatinine ratio suggest an upper GI bleed.",
   "Analyze findings in an upper GI bleed."),

mc(M("PHI.PA", UP, "prioritize_hypotheses", "moderate", GI, "ms"),
   "Which problem is the priority for this client at 0700?",
   [("Hypovolemia from ongoing blood loss", True, "Hypotension and tachycardia mean perfusion is failing."),
    ("Acute pain", False, "This is lower priority."),
    ("Knowledge deficit about NSAIDs", False, "Teaching comes later."),
    ("Risk for constipation", False, "This is lower priority.")],
   "Circulation threats from hemorrhage take priority.",
   "Prioritize hypovolemia in GI bleeding."),

sata(M("PHI.PA", UP, "generate_solutions", "moderate", GI, "ms"),
   "Which interventions should the nurse anticipate for this client?",
   [("Two large-bore IV lines", True, "These allow rapid fluid and blood replacement."),
    ("Type and crossmatch for blood", True, "Transfusion is likely."),
    ("IV proton pump inhibitor", True, "This reduces acid and stabilizes clots."),
    ("NPO status in preparation for endoscopy", True, "Endoscopy finds and treats the source."),
    ("Ibuprofen for back pain", False, "NSAIDs worsen ulcer bleeding."),
    ("Oral iron right away", False, "This isn't an acute treatment and can make stool black, confusing assessment.")],
   "Upper GI bleed: access, blood, PPI, NPO and urgent endoscopy.",
   "Plan interventions for an upper GI bleed."),

mc(M("PHI.PPT", "blood-transfusion-reactions", "take_action", "moderate", GI, "ms"),
   "Ten minutes into the first unit of packed red blood cells, the client reports chills and flank pain. What should the nurse do first?",
   [("Stop the transfusion and keep the vein open with 0.9% sodium chloride through new tubing", True, "These may be signs of an acute hemolytic reaction."),
    ("Slow the transfusion", False, "Any suspected reaction means stopping."),
    ("Give acetaminophen and continue", False, "This could be fatal."),
    ("Take the unit back to the blood bank before assessing the client", False, "The client comes first.")],
   "Suspected transfusion reaction: stop, keep the line open with saline, notify and monitor.",
   "Respond to a transfusion reaction during hemorrhage care."),

matrix_mc(M("PHI.PA", UP, "evaluate_outcomes", "difficult", GI, "ms"),
   "Review the 0900 findings. For each, indicate whether it shows improvement or needs immediate follow-up.",
   ["Improvement", "Needs immediate follow-up"],
   [("Heart rate 98/min", 0, ["Tachycardia is improving after transfusion.", "This is improving."]),
    ("Blood pressure 112/68 mm Hg", 0, ["Perfusion is improving.", "This is improving."]),
    ("Hemoglobin 9.0 g/dL", 0, ["The hemoglobin rose after transfusion.", "This is improving."]),
    ("New 300 mL coffee-ground vomit at 0855", 1, ["This isn't improvement.", "This suggests rebleeding; notify the provider right away."])],
   "Watch for rebleeding even as the vital signs improve.",
   "Evaluate outcomes in GI bleeding."),
]

# ---- Case 2: COPD exacerbation -------------------------------------------------------------------
copd_tabs = [
    ("copd-notes", "Nurses' notes", [
        note("Emergency department, 1400",
             "A 72-year-old with COPD (50 pack-years) has had increasing shortness of breath for 3 days, with a cough producing thick yellow sputum. Sitting upright and leaning forward on the bedside table. Speaks in 2- to 3-word phrases. Using neck muscles to breathe. Barrel-shaped chest. Temperature 38.1 °C.", id="copd-note-1400")]),
    ("copd-vitals", "Vital signs", [
        flow("Vital signs", ["1400", "1600"], [
            ("Respiratory rate (/min)", ["30", "22"]),
            ("Heart rate (/min)", ["112", "96"]),
            ("SpO2 (%)", ["84 room air", "90 on BiPAP"]),
            ("Blood pressure (mm Hg)", ["148/88", "136/80"])], id="copd-flow")]),
    ("copd-labs", "Arterial blood gases and labs", [
        table("Arterial blood gases", ["1400", "1600"], [
            ("pH", ["7.30", "7.36"]),
            ("PaCO2 (mm Hg)", ["62", "52"]),
            ("HCO3 (mEq/L)", ["30", "30"]),
            ("PaO2 (mm Hg)", ["52", "60"]),
            ("Blood glucose (mg/dL)", ["124", "298"])], id="copd-abg")]),
]

copd_items = [
hl_text(M("PHI.PA", AC, "recognize_cues", "moderate", R, "ms"),
   "Highlight the findings in the emergency department note that show increased work of breathing or a worsening condition.",
   ["The client is ",
    ("sitting upright and leaning forward on the bedside table", True, "The tripod position shows respiratory distress."),
    ", ",
    ("speaks in 2- to 3-word phrases", True, "Being unable to finish sentences shows severe dyspnea."),
    " and is ",
    ("using neck muscles to breathe", True, "Accessory muscle use shows increased work of breathing."),
    ". The client has a ",
    ("barrel-shaped chest", False, "This is a chronic COPD finding, not a sign of acute worsening."),
    " and a cough producing ",
    ("thick yellow sputum", True, "Purulent sputum suggests infection triggering the exacerbation."),
    "."],
   "Separate chronic COPD findings from signs of acute distress.",
   "Recognize cues of a COPD exacerbation."),

dtable(M("PHI.PA", "acid-base-imbalances", "analyze_cues", "moderate", R, "ms"),
   "For each 1400 arterial blood gas value, select the interpretation.",
   ["Value", "Result", "Interpretation"],
   [(["pH", "7.30"], level("Low", "A pH below 7.35 is acidemia.")),
    (["PaCO2", "62 mm Hg"], level("High", "PaCO2 above 45 shows hypoventilation.")),
    (["HCO3", "30 mEq/L"], level("High", "A raised HCO3 shows the kidneys compensating for chronic CO2 retention."))],
   "pH 7.30, PaCO2 62 and HCO3 30 show acute-on-chronic respiratory acidosis.",
   "Interpret ABG values in a COPD exacerbation."),

cloze(M("PHI.PA", AC, "prioritize_hypotheses", "difficult", R, "ms"),
   "Complete the sentence.",
   "The client's findings indicate {{b1}}, and the nurse should anticipate {{b2}}.",
   [[("acute-on-chronic respiratory acidosis from a COPD exacerbation", True, "Rising CO2 with a partly compensated pH shows acute worsening of chronic retention."),
     ("metabolic acidosis from sepsis", False, "The primary problem is high CO2, not low HCO3."),
     ("respiratory alkalosis from anxiety", False, "The pH is acidic.")],
    [("noninvasive positive pressure ventilation (BiPAP)", True, "BiPAP improves ventilation and lowers CO2, often avoiding intubation."),
     ("100% oxygen by nonrebreather mask", False, "Excess oxygen can worsen hypercapnia in COPD."),
     ("a sedative to ease anxiety", False, "Sedatives depress breathing.")]],
   "BiPAP is first-line for hypercapnic COPD exacerbations.",
   "Prioritize hypercapnic respiratory failure."),

matrix_mc(M("PHI.PA", AC, "generate_solutions", "moderate", R, "ms ph"),
   "For each intervention, indicate whether it is indicated or not indicated for this client.",
   ["Indicated", "Not indicated"],
   [("Titrate oxygen to SpO2 88% to 92%", 0, ["This avoids worsening hypercapnia.", "Controlled oxygen is indicated."]),
    ("Nebulized short-acting bronchodilators", 0, ["These relieve bronchospasm.", "These are indicated."]),
    ("Systemic corticosteroids", 0, ["Steroids shorten exacerbations.", "These are indicated."]),
    ("Antibiotics", 0, ["Fever and purulent sputum suggest bacterial infection.", "These are indicated."]),
    ("IV lorazepam for anxiety", 1, ["Sedatives can worsen hypoventilation.", "This isn't indicated."]),
    ("Nonrebreather mask to keep SpO2 at 100%", 1, ["Excess oxygen may worsen CO2 retention.", "This isn't indicated."])],
   "COPD exacerbation: controlled oxygen, bronchodilators, steroids, antibiotics if infected, BiPAP.",
   "Select treatments for a COPD exacerbation."),

mc(M("PHI.PA", AC, "take_action", "difficult", R, "ms"),
   "While on BiPAP, the client suddenly begins to vomit. What should the nurse do first?",
   [("Remove the mask right away and turn the client to the side", True, "This prevents aspiration of vomit into the lungs."),
    ("Increase the BiPAP pressure", False, "This raises the risk of aspiration."),
    ("Give an antiemetic and leave the mask on", False, "Vomiting into a sealed mask risks aspiration."),
    ("Lay the client flat", False, "This raises the risk of aspiration.")],
   "BiPAP: remove the mask for vomiting; watch for gastric distention and skin breakdown.",
   "Respond to vomiting during BiPAP."),

matrix_mc(M("PHI.PA", AC, "evaluate_outcomes", "moderate", R, "ms"),
   "Review the 1600 findings. For each, indicate whether it shows improvement or needs follow-up.",
   ["Improvement", "Needs follow-up"],
   [("Respiratory rate 22/min", 0, ["The work of breathing has decreased.", "This is improving."]),
    ("pH 7.36 and PaCO2 52 mm Hg", 0, ["Ventilation is improving.", "This is improving."]),
    ("SpO2 90%", 0, ["This is within the 88% to 92% target.", "This meets the target."]),
    ("Blood glucose 298 mg/dL", 1, ["This isn't improvement.", "Steroid-induced hyperglycemia needs treatment and monitoring."])],
   "Evaluate the respiratory response and watch for side effects of treatment such as steroid hyperglycemia.",
   "Evaluate outcomes in a COPD exacerbation."),
]

BUILT = build(37, "ngn-cases-2", "NGN case studies II: upper GI bleed and COPD exacerbation", [], cases=[
    case("Upper gastrointestinal bleed", gi_tabs, gi_items),
    case("COPD exacerbation", copd_tabs, copd_items),
])
