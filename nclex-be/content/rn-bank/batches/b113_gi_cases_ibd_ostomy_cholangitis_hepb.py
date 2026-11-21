"""Batch 113 (case conversion, chunk 4: gastrointestinal): four full NGN case studies (CASE-MAP.md).

Cases: a Crohn disease flare treated with infliximab; a high-output ileostomy after colectomy for ulcerative colitis;
acute cholangitis treated with ERCP; acute hepatitis B with exposure management. Each case walks the six CJMM steps
in order, unfolds with new data in steps 4 to 6, and includes a management of care, safety or health promotion
question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC = "PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC"
GI = "gastrointestinal"
BOWEL = "bowel-disorders"
IMMUNO = "immunosuppressant-therapy"
OSTOMY = "ostomy-care"
FLUID = "fluid-volume-imbalances"
GALL = "pancreas-and-gallbladder-disorders"
GI_DX = "gi-diagnostic-procedures"
SHOCK = "shock-and-sepsis"
LIVER = "liver-disorders"
HEP_PREV = "hepatitis-prevention"
SHARPS = "occupational-exposure-and-sharps-safety"
TEACH = "health-teaching-and-behavior-change"
NUTR = "nutrition-and-therapeutic-diets"
COLLAB = "collaboration-and-referrals"
CONFID = "confidentiality-and-information-security"

items = []

# ================================================================================================
# Case 1: Crohn disease flare and infliximab
# ================================================================================================
cd_tabs = [
    ("cd-notes", "Nurses' notes", [note("GI clinic and infusion center",
        "A 29-year-old client with Crohn disease of the terminal ileum has had 6 loose stools a day, cramping in the "
        "right lower abdomen and a 5 kg weight loss over a month. The client has a draining opening near the anus. "
        "The client smokes. The gastroenterologist plans infliximab infusions.", id="cd-note")]),
    ("cd-labs", "Labs and screening", [table("Labs and screening", ["Result", "Reference"], [
        ("Hemoglobin", ["10.2 g/dL, MCV 108", "12 to 16; MCV 80 to 100"]),
        ("Vitamin B12", ["150 pg/mL", "200 to 900"]),
        ("C-reactive protein", ["48 mg/L", "below 10"]),
        ("Albumin", ["2.9 g/dL", "3.5 to 5.0"]),
        ("Interferon-gamma release assay (TB)", ["Positive", "-"]),
        ("Hepatitis B surface antigen", ["Negative", "-"])], id="cd-table")]),
]

cd_qs = [
hl_table(M(PA, BOWEL, "recognize_cues", "moderate", GI, "ms"),
   "Highlight the findings that require follow-up before starting infliximab.",
   ["Finding", "Result"],
   [(["TB screening", "Positive IGRA"], True, "Latent TB can reactivate on anti-TNF therapy; it must be treated first."),
    (["Hemoglobin / MCV", "10.2 g/dL, MCV 108"], True, "Macrocytic anemia."),
    (["Vitamin B12", "150 pg/mL"], True, "The terminal ileum absorbs B12."),
    (["Perianal opening", "Draining"], True, "A fistula; an abscess must be drained before anti-TNF therapy."),
    (["Hepatitis B surface antigen", "Negative"], False, "Reassuring; reactivation risk is low."),
    (["Smoking", "Yes"], True, "Smoking worsens Crohn disease.")],
   "Latent TB, a draining fistula, B12 deficiency and smoking all need action before or with biologic therapy.",
   "Recognize cues before biologic therapy in Crohn disease."),

matrix_mc(M(PA, BOWEL, "analyze_cues", "difficult", GI, "ms"),
   "For each finding, indicate whether it is more typical of Crohn disease or ulcerative colitis.",
   ["Crohn disease", "Ulcerative colitis"],
   [("Disease in the terminal ileum with skip areas", 0, "Crohn disease can affect any part of the gut, in patches."),
    ("Fistulas", 0, "Full-thickness inflammation forms fistulas."),
    ("Continuous inflammation starting at the rectum", 1, "Typical of UC."),
    ("Bloody diarrhea as the main symptom", 1, "Typical of UC."),
    ("B12 deficiency", 0, "From ileal disease or resection."),
    ("Smoking worsens the disease", 0, "Smoking worsens Crohn disease.")],
   "Crohn disease is patchy and full thickness, often in the ileum, with fistulas and B12 deficiency.",
   "Analyze Crohn disease against ulcerative colitis."),

cloze(M(PPT, IMMUNO, "prioritize_hypotheses", "difficult", GI, "ph"),
   "Complete the sentences by choosing from the lists of options.",
   "Before the first infliximab dose, the priority is to {{b1}}. Starting it without this could cause {{b2}}.",
   [[("start treatment for latent tuberculosis", True, "Usually at least 1 month of latent TB treatment first."),
     ("give a live vaccine", False, "Live vaccines are avoided on anti-TNF therapy."),
     ("check a stool culture", False, "Not the main issue."),
     ("start a low-residue diet", False, "Not the priority.")],
    [("reactivation of tuberculosis", True, "TNF blockade weakens the granulomas that contain TB."),
     ("hypoglycemia", False, "Not a risk."),
     ("kidney stones", False, "Not the risk."),
     ("hair loss", False, "Not the main risk.")]],
   "A positive TB screen must be treated before anti-TNF therapy.",
   "Prioritize safety before infliximab."),

sata(M(PPT, IMMUNO, "generate_solutions", "difficult", GI, "ph"),
   "Which parts of the infusion plan should the nurse anticipate?",
   [("Premedication as prescribed, such as acetaminophen and an antihistamine", True, "Reduces infusion reactions."),
    ("Vital signs before and during the infusion", True, "Detects reactions."),
    ("Emergency drugs, including epinephrine, available", True, "Anaphylaxis is possible."),
    ("B12 injections", True, "Corrects deficiency from ileal disease."),
    ("Give the measles-mumps-rubella vaccine on the same day", False, "Live vaccine is contraindicated."),
    ("Infuse rapidly over 15 minutes to save chair time", False, "Given over at least 2 hours.")],
   "Premedicate, monitor, have emergency drugs ready, correct B12, and avoid live vaccines.",
   "Generate the infliximab infusion plan.",
   ex=[note("Plan", "Isoniazid with pyridoxine for 1 month, then infliximab 5 mg/kg. Smoking cessation. B12 "
       "injections. Colorectal surgery to examine the fistula.", id="cd-plan")]),

sata(M(PPT, IMMUNO, "take_action", "difficult", GI, "ph"),
   "Thirty minutes into the second infusion, review the findings. Which actions should the nurse take?",
   [("Stop the infusion", True, "Signs of an infusion reaction."),
    ("Keep the IV line open with 0.9% sodium chloride", True, "Keeps access for drugs."),
    ("Check vital signs and airway", True, "Assess severity."),
    ("Notify the provider", True, "Plan treatment and future infusions."),
    ("Slow the infusion and continue", False, "With wheezing and hypotension, stop."),
    ("Give the remaining dose by IV push", False, "Dangerous.")],
   "An infusion reaction with wheezing and low pressure means stopping, keeping access and treating.",
   "Take action for an infliximab infusion reaction.",
   ex=[flow("Second infusion", ["Start", "30 minutes"], [
       ("Client", ["Comfortable", "Flushed, itchy, chest tight"]),
       ("Breathing", ["Clear", "Wheezing"]),
       ("Blood pressure", ["122/76", "94/58 mm Hg"]),
       ("Heart rate", ["78", "112/min"])], id="cd-reaction")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", GI, "ph ms"),
   "At the 3-month visit, for each statement or finding, indicate whether it shows the plan is working or needs "
   "follow-up.",
   ["Working", "Needs follow-up"],
   [("Two formed stools a day; weight up 3 kg", 0, "Remission."),
    ("CRP 6 mg/L", 0, "Inflammation controlled."),
    ("\"I'll call if I get a fever or a cough that won't go away.\"", 0, "Infection warning signs on a biologic."),
    ("\"I'm still smoking a pack a day.\"", 1, "Smoking increases flares and surgery."),
    ("\"I got the nasal flu vaccine at the pharmacy.\"", 1, "The nasal vaccine is live; inactivated injection is used."),
    ("Fistula drainage has stopped", 0, "Healing.")],
   "Remission and fistula healing show success; smoking and a live vaccine need follow-up.",
   "Evaluate Crohn disease treatment.",
   ex=[note("3-month visit", "Infusions continued every 8 weeks with premedication and a slower rate.", id="cd-m3")]),
]

# ================================================================================================
# Case 2: High-output ileostomy after colectomy
# ================================================================================================
ost_tabs = [
    ("ost-notes", "Nurses' notes", [note("Surgical unit, postoperative day 4",
        "A 34-year-old client had a total colectomy with an end ileostomy for severe ulcerative colitis. The client is "
        "eating a regular diet and drinking plenty of water and sports drinks. The client feels dizzy when standing. "
        "The stoma is pink and moist.", id="ost-note")]),
    ("ost-data", "Intake, output and labs", [table("Intake, output and labs", ["Day 4", "Reference"], [
        ("Ileostomy output", ["2,400 mL / 24 hours, watery", "about 500 to 1,200 mL once settled"]),
        ("Urine output", ["520 mL / 24 hours, dark", "-"]),
        ("Oral intake", ["3,000 mL, mostly water", "-"]),
        ("Sodium / potassium", ["131 / 3.2 mEq/L", "-"]),
        ("Magnesium", ["1.4 mg/dL", "1.5 to 2.5"]),
        ("Creatinine", ["1.5 mg/dL (0.8 before surgery)", "-"]),
        ("Orthostatic BP", ["118/72 lying, 96/60 standing", "-"])], id="ost-table")]),
]

ost_qs = [
hl_table(M(BCC, OSTOMY, "recognize_cues", "moderate", GI, "ms"),
   "Highlight the findings that require follow-up.",
   ["Finding", "Day 4"],
   [(["Ileostomy output", "2,400 mL, watery"], True, "High output (above about 1,500 mL a day) risks dehydration."),
    (["Urine output", "520 mL, dark"], True, "The kidneys are conserving water."),
    (["Creatinine", "1.5 mg/dL"], True, "Prerenal kidney injury."),
    (["Electrolytes", "Na 131, K 3.2, Mg 1.4"], True, "Losses in ileostomy fluid."),
    (["Orthostatic BP", "Falls 22 mm Hg on standing"], True, "Volume depletion."),
    (["Stoma", "Pink and moist"], False, "A healthy stoma.")],
   "High ileostomy output with kidney injury, electrolyte losses and orthostasis.",
   "Recognize cues of a high-output ileostomy."),

cloze(M(PA, FLUID, "analyze_cues", "difficult", GI, "ms"),
   "Complete the sentences by choosing from the lists of options.",
   "The client's dehydration is made worse by {{b1}}, because {{b2}}.",
   [[("drinking large amounts of plain water", True, "Hypotonic fluid draws sodium into the gut and increases output."),
     ("eating a regular diet", False, "Food thickens output."),
     ("a pink stoma", False, "Not related."),
     ("walking", False, "Not related.")],
    [("water without salt pulls sodium into the bowel and is lost through the stoma", True,
      "An oral rehydration solution with enough sodium is absorbed better."),
     ("water is fully absorbed in the small bowel", False, "Not after colectomy."),
     ("sports drinks contain too much sodium", False, "Most contain too little."),
     ("the colon is absorbing the water", False, "The colon was removed.")]],
   "Without a colon, plain water increases ileostomy losses; oral rehydration solution is needed.",
   "Analyze fluid losses after ileostomy."),

matrix_mc(M(PA, FLUID, "prioritize_hypotheses", "difficult", GI, "ms"),
   "For each problem, indicate its priority.",
   ["High priority", "Lower priority"],
   [("Dehydration with acute kidney injury", 0, "Can progress quickly."),
    ("Hypokalemia and hypomagnesemia", 0, "Dysrhythmia risk."),
    ("Falls from orthostatic hypotension", 0, "Safety risk."),
    ("Body image concerns about the stoma", 1, "Important; addressed once stable.")],
   "Fluid, electrolytes and falls come first; body image is addressed soon after.",
   "Prioritize problems with a high-output ileostomy."),

sata(M(BCC, NUTR, "generate_solutions", "difficult", GI, "ms"),
   "Which interventions should the nurse anticipate?",
   [("IV fluids with electrolyte replacement", True, "Restores volume and electrolytes."),
    ("An oral rehydration solution instead of plain water", True, "Better absorbed."),
    ("Thickening foods such as bananas, rice, applesauce and pasta", True, "Slow output."),
    ("Loperamide before meals as prescribed", True, "Slows transit."),
    ("Encourage more plain water and sugary drinks", False, "Increases output."),
    ("A high-fiber diet with raw vegetables right away", False, "Raises output and blockage risk early on.")],
   "Replace losses, use oral rehydration solution and thickening foods, and slow transit.",
   "Generate solutions for a high-output ileostomy.",
   ex=[note("Orders", "IV 0.9% sodium chloride with KCl. Magnesium sulfate IV. Oral rehydration solution. Loperamide "
       "before meals. Wound, ostomy and continence nurse consult.", id="ost-orders")]),

sata(M(BCC, OSTOMY, "take_action", "difficult", GI, "ms"),
   "The client is ready to learn pouch care. Which teaching should the nurse include?",
   [("Empty the pouch when it is one-third to one-half full", True, "Prevents leaks and detachment."),
    ("Cut the barrier opening 1/8 inch (3 mm) larger than the stoma", True, "Protects skin from enzyme-rich output."),
    ("Report a stoma that turns dark red, purple or black", True, "Signs of poor blood supply."),
    ("Ask before taking extended-release tablets", True, "Absorption is limited."),
    ("Expect formed stools from the stoma once it has fully healed", False, "Ileostomy output stays liquid to pasty."),
    ("Irrigate the ileostomy each morning to regulate the output", False, "Ileostomies are not irrigated.")],
   "Empty early, fit the barrier closely, watch stoma color and review medicines.",
   "Teach ileostomy care.",
   ex=[flow("Day 6", ["Day 6"], [
       ("Ileostomy output", ["1,100 mL / 24 hours, thicker"]),
       ("Creatinine", ["0.9 mg/dL"]),
       ("Skin", ["Red, weepy skin around the stoma"])], id="ost-day6")]),

matrix_mc(M(MOC, COLLAB, "evaluate_outcomes", "difficult", GI, "ms mgmt"),
   "Before discharge, for each finding, indicate whether it shows readiness or needs follow-up.",
   ["Ready", "Needs follow-up"],
   [("Output 900 to 1,100 mL a day, pasty", 0, "Settled output."),
    ("Changes the pouch independently with a good seal", 0, "Self-care achieved."),
    ("Skin around the stoma red and weepy", 1, "Barrier opening or leak needs review by the ostomy nurse."),
    ("\"I'll drink oral rehydration solution if output goes up or it's hot.\"", 0, "Prevents dehydration."),
    ("\"I'm not going back to the gym, everyone will know.\"", 1, "Body image concern; offer support and an ostomy support group."),
    ("Home supplies and ostomy nurse follow-up arranged", 0, "Continuity of care.")],
   "Self-care and fluid plans are in place; skin problems and body image need follow-up through the team.",
   "Evaluate readiness for discharge with an ileostomy.",
   ex=[note("Day 8", "Planned discharge tomorrow.", id="ost-day8")]),
]

# ================================================================================================
# Case 3: Acute cholangitis and ERCP
# ================================================================================================
chol_tabs = [
    ("chol-notes", "Nurses' notes", [note("Emergency department, 2200",
        "A 68-year-old client with known gallstones has had right upper abdominal pain for 2 days, now with shaking "
        "chills. The client's eyes are yellow and the urine is dark like tea; stools are pale. The client takes "
        "warfarin for atrial fibrillation.", id="chol-note")]),
    ("chol-labs", "Vital signs and labs", [table("Vital signs and labs", ["2200", "Reference"], [
        ("Temperature", ["39.4 C (102.9 F)", "-"]),
        ("Blood pressure", ["94/56 mm Hg", "-"]),
        ("Heart rate", ["118/min", "-"]),
        ("Total bilirubin", ["5.8 mg/dL", "0.3 to 1.2"]),
        ("Alkaline phosphatase", ["420 units/L", "40 to 120"]),
        ("WBC", ["19,400/mm3", "-"]),
        ("INR", ["2.8", "-"]),
        ("Lipase", ["38 units/L", "0 to 160"])], id="chol-table")]),
]

chol_qs = [
hl_text(M(PA, GALL, "recognize_cues", "moderate", GI, "ms"),
   "Highlight the findings that require immediate action.",
   ["The client has ",
    ("right upper abdominal pain", True, "Part of Charcot triad."),
    " with ",
    ("shaking chills", True, "Fever and rigors; bacteremia."),
    " and ",
    ("yellow eyes", True, "Jaundice from blocked bile flow."),
    ". The client ",
    ("has known gallstones", False, "A risk factor, not an acute sign."),
    ". The blood pressure is ",
    ("94/56 mm Hg", True, "Hypotension suggests septic shock."),
    "."],
   "Pain, fever and jaundice (Charcot triad) with hypotension point to cholangitis with sepsis.",
   "Recognize cues of acute cholangitis."),

matrix_mc(M(PA, GALL, "analyze_cues", "difficult", GI, "ms lab"),
   "For each finding, indicate what it reflects.",
   ["Blocked bile duct", "Infection and sepsis", "Not related to this illness"],
   [("Bilirubin 5.8 mg/dL", 0, "Bile cannot drain."),
    ("Alkaline phosphatase 420 units/L", 0, "Rises with bile duct obstruction."),
    ("Dark urine and pale stools", 0, "Conjugated bilirubin goes to the urine, not the stool."),
    ("Temperature 39.4 C and WBC 19,400", 1, "Infected bile."),
    ("Blood pressure 94/56 mm Hg", 1, "Septic shock developing."),
    ("Lipase 38 units/L", 2, "Normal; pancreatitis is not present.")],
   "A stone blocking the common bile duct has caused infection and sepsis.",
   "Analyze cholangitis findings."),

cloze(M(PA, SHOCK, "prioritize_hypotheses", "difficult", GI, "ms"),
   "Complete the sentence by choosing from the lists of options.",
   "The client needs {{b1}}, and the INR of 2.8 matters because {{b2}}.",
   [[("antibiotics, fluids and urgent drainage of the bile duct", True, "Source control by ERCP."),
     ("a low-fat diet", False, "Not for acute cholangitis."),
     ("elective cholecystectomy in 6 weeks", False, "Later, after the acute episode."),
     ("oral antibiotics at home", False, "Unsafe.")],
    [("ERCP with sphincterotomy carries a bleeding risk", True, "Warfarin may need reversal before the procedure."),
     ("it causes the jaundice", False, "Not the cause."),
     ("it is in the normal range", False, "It is therapeutic but raises procedural bleeding risk."),
     ("it rules out sepsis", False, "Not related.")]],
   "Cholangitis with sepsis needs antibiotics, fluids and ERCP drainage, with anticoagulation managed first.",
   "Prioritize cholangitis care."),

sata(M(RRP, GI_DX, "generate_solutions", "difficult", GI, "ms ph"),
   "Which preparations for ERCP should the nurse anticipate?",
   [("Blood cultures before antibiotics", True, "Identify the organism."),
    ("IV piperacillin-tazobactam within the hour", True, "Covers gut organisms."),
    ("Hold warfarin and give vitamin K as prescribed", True, "Lowers bleeding risk."),
    ("NPO", True, "Sedation and aspiration risk."),
    ("Confirm informed consent", True, "Invasive procedure."),
    ("Give a fatty meal to stimulate the gallbladder", False, "Not appropriate.")],
   "Cultures, antibiotics, anticoagulation reversal, NPO and consent before ERCP.",
   "Generate ERCP preparation.",
   ex=[note("Plan", "ERCP in the morning or sooner if unstable. Fluid bolus 30 mL/kg.", id="chol-plan")]),

sata(M(RRP, GI_DX, "take_action", "difficult", GI, "ms"),
   "Four hours after ERCP with stone removal, review the findings. Which actions should the nurse take?",
   [("Notify the provider", True, "New severe pain radiating to the back after ERCP suggests pancreatitis."),
    ("Keep the client NPO", True, "Rests the pancreas."),
    ("Check vital signs and continue IV fluids", True, "Supports perfusion."),
    ("Give the prescribed analgesia", True, "Pain control."),
    ("Resume a regular diet as planned", False, "Not with suspected pancreatitis."),
    ("Restart warfarin tonight as scheduled", False, "Needs a plan after sphincterotomy and possible bleeding.")],
   "New epigastric pain to the back with a high lipase after ERCP is post-ERCP pancreatitis.",
   "Take action for a complication after ERCP.",
   ex=[flow("4 hours after ERCP", ["Post-ERCP"], [
       ("Pain", ["Severe epigastric pain radiating to the back"]),
       ("Lipase", ["1,840 units/L"]),
       ("Temperature", ["37.6 C (99.7 F)"]),
       ("Heart rate", ["108/min"]),
       ("Bilirubin", ["3.1 mg/dL"])], id="chol-post")]),

matrix_mc(M(PA, GALL, "evaluate_outcomes", "difficult", GI, "ms"),
   "Review the findings 48 hours later. For each finding, indicate whether it shows improvement or needs follow-up.",
   ["Shows improvement", "Needs follow-up"],
   [("Afebrile; blood pressure 126/74 mm Hg", 0, "Sepsis resolved."),
    ("Bilirubin 1.6 mg/dL", 0, "Bile is draining."),
    ("Lipase 220 units/L, pain 2 of 10", 0, "Pancreatitis settling."),
    ("Tolerating clear liquids", 0, "Diet advancing."),
    ("Black, tarry stool and hemoglobin down 2 g/dL", 1, "Bleeding from the sphincterotomy site."),
    ("No plan yet for gallbladder removal", 1, "Cholecystectomy is advised to prevent recurrence.")],
   "Sepsis, obstruction and pancreatitis are improving; bleeding and the plan for cholecystectomy need follow-up.",
   "Evaluate outcomes after ERCP.",
   ex=[flow("48 hours", ["48 h"], [
       ("Vital signs", ["Afebrile, BP 126/74 mm Hg"]),
       ("Bilirubin", ["1.6 mg/dL"]),
       ("Lipase / pain", ["220 units/L / 2 of 10"]),
       ("Diet", ["Clear liquids"]),
       ("Stool", ["One black, tarry stool"]),
       ("Hemoglobin", ["10.1 g/dL (12.1 on admission)"])], id="chol-48h")]),
]

# ================================================================================================
# Case 4: Acute hepatitis B
# ================================================================================================
hepb_tabs = [
    ("hepb-notes", "Nurses' notes", [note("Medical unit",
        "A 31-year-old client has had fatigue, nausea, right upper abdominal ache and dark urine for a week, and now "
        "yellow skin. The client shares a home with a partner and a 6-month-old infant. During admission, a nurse "
        "sustained a needlestick from a used needle after drawing the client's blood.", id="hepb-note")]),
    ("hepb-labs", "Labs", [table("Labs", ["Result", "Reference"], [
        ("HBsAg", ["Positive", "-"]),
        ("IgM anti-HBc", ["Positive", "-"]),
        ("Anti-HBs", ["Negative", "-"]),
        ("ALT", ["1,850 units/L", "7 to 56"]),
        ("Total bilirubin", ["7.2 mg/dL", "0.3 to 1.2"]),
        ("INR", ["1.4", "0.8 to 1.1"]),
        ("Glucose", ["88 mg/dL", "-"])], id="hepb-table")]),
]

hepb_qs = [
hl_table(M(PA, LIVER, "recognize_cues", "moderate", GI, "ms cdc"),
   "Highlight the findings that require follow-up.",
   ["Finding", "Result"],
   [(["HBsAg and IgM anti-HBc", "Positive"], True, "Acute hepatitis B infection."),
    (["ALT", "1,850 units/L"], True, "Severe liver cell injury."),
    (["INR", "1.4"], True, "Watch for liver failure; a rising INR is a warning sign."),
    (["Household", "Partner and infant"], True, "Close contacts need protection."),
    (["Nurse", "Needlestick after drawing blood"], True, "Occupational exposure."),
    (["Glucose", "88 mg/dL"], False, "Normal; hypoglycemia would suggest failure.")],
   "Acute hepatitis B with severe liver injury and exposures of family and staff.",
   "Recognize cues of acute hepatitis B."),

matrix_mc(M(RRP, "laboratory-values-and-monitoring", "analyze_cues", "difficult", GI, "cdc lab"),
   "For each marker, indicate what it means.",
   ["Current infection", "Recent (acute) infection", "Immunity"],
   [("HBsAg positive", 0, "The virus is present."),
    ("IgM anti-HBc positive", 1, "Shows infection within about 6 months."),
    ("Anti-HBs positive (in someone else)", 2, "Immunity from vaccine or recovery."),
    ("HBsAg positive for longer than 6 months (in someone else)", 0, "Chronic infection.")],
   "HBsAg means current infection, IgM anti-HBc means acute, anti-HBs means immunity.",
   "Analyze hepatitis B serology."),

cloze(M(PA, LIVER, "prioritize_hypotheses", "difficult", GI, "ms"),
   "Complete the sentence by choosing from the lists of options.",
   "The nurse should watch most closely for {{b1}}, shown by {{b2}}.",
   [[("acute liver failure", True, "A small share of acute hepatitis B becomes fulminant."),
     ("hepatocellular cancer this week", False, "A long-term risk."),
     ("gallstones", False, "Not related."),
     ("kidney stones", False, "Not related.")],
    [("rising INR, confusion or hypoglycemia", True, "Signs of failing synthetic function."),
     ("falling ALT with improving symptoms", False, "Recovery."),
     ("dark urine", False, "Already present."),
     ("fatigue", False, "Expected.")]],
   "Watch for liver failure: INR, mental status and glucose.",
   "Prioritize monitoring in acute hepatitis."),

grouping(M(SIC, SHARPS, "generate_solutions", "difficult", GI, "cdc"),
   "For each person exposed, select the actions the plan should include.",
   [("The nurse with the needlestick (vaccinated, immune status unknown)", [
       ("Wash the site with soap and water at once", True, "First aid."),
       ("Report to occupational health immediately", True, "Prophylaxis works best within 24 hours."),
       ("Test anti-HBs and give HBIG and a vaccine dose if not immune", True, "Post-exposure prophylaxis."),
       ("Squeeze and soak the wound in bleach", False, "Not recommended.")]),
    ("The infant (unvaccinated)", [
       ("Hepatitis B vaccine and HBIG", True, "Protects an exposed household infant."),
       ("No action because infants are not at risk", False, "Infants are at high risk of chronic infection.")]),
    ("The partner", [
       ("Test, and vaccinate if not immune", True, "Sexual contact is a route."),
       ("Use barrier protection until immunity is confirmed", True, "Reduces transmission.")])],
   "Exposed people need testing, vaccination and, for some, immune globulin; the needlestick needs immediate reporting.",
   "Generate an exposure plan for hepatitis B.",
   ex=[note("Exposure report", "The nurse completed the hepatitis B series years ago; titer never checked.",
       id="hepb-exposure")]),

sata(M(MOC, CONFID, "take_action", "difficult", GI, "mgmt"),
   "The client's employer calls the unit asking whether the client has hepatitis. The partner asks the nurse what "
   "the diagnosis is. Which actions are appropriate?",
   [("Tell the employer that no information can be shared without the client's permission", True, "Confidentiality."),
    ("Ask the client whether the partner may be told the diagnosis", True, "The client decides who is told."),
    ("Encourage the client to tell the partner, who needs testing", True, "Protects the partner."),
    ("Let the public health department handle required reporting", True, "Hepatitis B is reportable."),
    ("Tell the partner the diagnosis because they live together", False, "Not without the client's consent."),
    ("Confirm the diagnosis to the employer to protect coworkers", False, "A breach of confidentiality.")],
   "Share health information only with consent; required public health reporting goes through official channels.",
   "Take action to protect confidentiality.",
   ex=[note("Day 2", "ALT 1,520 units/L; INR 1.3. The client is resting.", id="hepb-day2")]),

matrix_mc(M(HPM, HEP_PREV, "evaluate_outcomes", "difficult", GI, "cdc"),
   "Before discharge, for each statement, indicate whether it shows understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"I won't share razors or toothbrushes.\"", 0, "Blood contact spreads hepatitis B."),
    ("\"I'll avoid alcohol and ask before taking acetaminophen.\"", 0, "Protects the liver."),
    ("\"I'll come back in 6 months to check whether the virus has cleared.\"", 0, "Testing for chronic infection."),
    ("\"Hepatitis B spreads by sharing a cup.\"", 1, "Not spread by casual contact or food."),
    ("\"I can donate blood once I feel better.\"", 1, "People with hepatitis B infection cannot donate blood."),
    ("\"My baby has had the vaccine and immune globulin.\"", 0, "Protection is in place.")],
   "Teaching covers blood exposure, liver protection, follow-up testing and correct transmission facts.",
   "Evaluate hepatitis B teaching.",
   ex=[note("Discharge", "ALT falling, INR 1.1. Infant given HBIG and vaccine.", id="hepb-dc")]),
]

cases = [
    case("Crohn disease flare and infliximab", cd_tabs, cd_qs),
    case("High-output ileostomy after colectomy", ost_tabs, ost_qs),
    case("Acute cholangitis and ERCP", chol_tabs, chol_qs),
    case("Acute hepatitis B and exposures", hepb_tabs, hepb_qs),
]

BUILT = build(113, "gi-cases-ibd-ostomy-cholangitis-hepb",
              "Gastrointestinal case studies: Crohn disease, ileostomy, cholangitis and hepatitis B",
              items, cases, case_conversion=True)
