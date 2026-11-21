"""Batch 106 (case conversion, chunk 2: pulmonary): four full NGN case studies (CASE-MAP.md).

Cases: a cystic fibrosis exacerbation treated with tobramycin; aspiration pneumonia after a stroke; an influenza
outbreak in a long-term care unit; a malignant pleural effusion with thoracentesis and goals-of-care decisions.
Each case walks the six CJMM steps in order, unfolds with new data in steps 4 to 6, and includes a management of
care or safety question."""
from qb import *

RRP, PA, PPT, BCC, MOC, SIC = "PHI.RRP", "PHI.PA", "PHI.PPT", "PHI.BCC", "SECE.MOC", "SECE.SIC"
RESP = "respiratory"
PNEU = "pneumonia-care"
ABX = "antimicrobial-therapy"
RESP_MEDS = "respiratory-medications"
TBP = "transmission-based-precautions"
HAI = "healthcare-associated-infection-prevention"
STROKE = "stroke-care"
NUTR = "nutrition-and-therapeutic-diets"
DELEG = "delegation-and-supervision"
PRIORITY = "prioritizing-client-care"
RESP_DX = "respiratory-diagnostic-procedures"
PALL = "palliative-and-end-of-life-care"
ADVANCE = "advance-directives"
LABS = "laboratory-values-and-monitoring"
CANCER = "cancer-care"

items = []

# ================================================================================================
# Case 1: Cystic fibrosis exacerbation
# ================================================================================================
cf_tabs = [
    ("cf-notes", "Nurses' notes", [note("Pulmonary unit, admission",
        "A 22-year-old client with cystic fibrosis is admitted with 2 weeks of more sputum, now thick and green, "
        "worse cough, fatigue and a 3 kg weight loss. FEV1 is 48% of predicted (65% three months ago). Sputum has "
        "grown Pseudomonas aeruginosa before. The client reports greasy, foul-smelling stools and has been "
        "\"skipping the enzyme capsules when eating on the go.\" Another client with cystic fibrosis is in a room "
        "on the same unit.", id="cf-note")]),
    ("cf-labs", "Laboratory results", [table("Laboratory results", ["Admission", "Reference range"], [
        ("WBC", ["14,200/mm3", "5,000 to 10,000/mm3"]),
        ("Glucose (random)", ["262 mg/dL", "70 to 140 mg/dL"]),
        ("HbA1c", ["7.4%", "below 5.7%"]),
        ("Creatinine", ["0.8 mg/dL", "0.6 to 1.2 mg/dL"]),
        ("Albumin", ["2.9 g/dL", "3.5 to 5.0 g/dL"])], id="cf-lab")]),
]

cf_qs = [
hl_table(M(PA, PNEU, "recognize_cues", "moderate", RESP, "ms peds"),
   "Highlight the findings that require follow-up.",
   ["Finding", "Value"],
   [(["FEV1", "48% of predicted, down from 65%"], True, "A large fall in lung function marks a pulmonary exacerbation."),
    (["Sputum", "More, thick and green"], True, "Increased purulent sputum with a history of Pseudomonas."),
    (["Stools", "Greasy and foul-smelling"], True, "Steatorrhea from missed pancreatic enzymes."),
    (["Glucose and HbA1c", "262 mg/dL; 7.4%"], True, "Points to cystic fibrosis-related diabetes."),
    (["Albumin", "2.9 g/dL"], True, "Malnutrition, which worsens outcomes."),
    (["Creatinine", "0.8 mg/dL"], False, "Normal; the baseline before nephrotoxic antibiotics.")],
   "An exacerbation in CF affects lungs, nutrition and glucose together.",
   "Recognize cues of a cystic fibrosis exacerbation."),

matrix_mc(M(PA, PNEU, "analyze_cues", "difficult", RESP, "ms"),
   "For each finding, indicate the problem it reflects.",
   ["Lung infection", "Pancreatic insufficiency", "Cystic fibrosis-related diabetes"],
   [("Thick green sputum with a fall in FEV1", 0, "Pseudomonas infection inflames and plugs the airways."),
    ("Greasy, foul-smelling stools", 1, "Fat is not absorbed without pancreatic enzymes."),
    ("Weight loss with low albumin", 1, "Malabsorption plus the energy cost of infection."),
    ("HbA1c 7.4%", 2, "Scarring of the pancreas reduces insulin production."),
    ("WBC 14,200/mm3", 0, "Shows active infection.")],
   "Cystic fibrosis affects the lungs and both the exocrine and endocrine pancreas.",
   "Analyze the systems involved in cystic fibrosis."),

cloze(M(PA, PNEU, "prioritize_hypotheses", "difficult", RESP, "ms"),
   "Complete the sentences by choosing from the lists of options.",
   "The client's most important problem is {{b1}}. The treatment most likely to improve the FEV1 is {{b2}} with "
   "{{b3}}.",
   [[("ineffective airway clearance from infected, thick secretions", True, "Mucus plugging with infection drives the fall in lung function."),
     ("hyperglycemia", False, "Needs treatment but does not explain the FEV1."),
     ("diarrhea", False, "Secondary to missed enzymes."),
     ("anxiety", False, "Not supported.")],
    [("IV antibiotics active against Pseudomonas", True, "For example, an IV beta-lactam plus tobramycin."),
     ("oral amoxicillin", False, "Does not cover Pseudomonas."),
     ("a cough suppressant", False, "Coughing clears secretions in CF; suppressing it is harmful."),
     ("bed rest", False, "Activity helps clearance.")],
    [("intensive airway clearance several times a day", True, "Chest physiotherapy, an oscillating vest and mucus-thinning inhalers."),
     ("fluid restriction", False, "Dehydration thickens secretions."),
     ("a low-calorie diet", False, "CF needs high calories."),
     ("lying flat to rest", False, "Does not clear secretions.")]],
   "Clearing infected mucus is the priority: antibiotics against Pseudomonas plus intensive airway clearance.",
   "Prioritize problems in a CF exacerbation."),

matrix_mc(M(SIC, TBP, "generate_solutions", "difficult", RESP, "cdc"),
   "Another client with cystic fibrosis is on the unit. For each measure, indicate whether it is indicated or "
   "contraindicated.",
   ["Indicated", "Contraindicated"],
   [("A private room with contact precautions", 0, "CF organisms such as Pseudomonas spread between clients with CF."),
    ("Keeping the two clients at least 6 feet apart at all times", 0, "Limits spread of CF pathogens."),
    ("Letting the two clients share the unit lounge to support each other", 1, "Cross-infection between CF clients is dangerous."),
    ("Wearing a mask when outside the room", 0, "Reduces droplet spread."),
    ("Sharing the same nebulizer cleaning station", 1, "Equipment can carry organisms between clients.")],
   "Clients with CF are separated from each other because their organisms spread easily and harm both.",
   "Generate infection-control measures for cystic fibrosis.",
   ex=[note("Plan", "IV ceftazidime and tobramycin, airway clearance four times daily, dornase alfa, nutrition "
       "consult, endocrinology consult.", id="cf-plan")]),

sata(M(PPT, ABX, "take_action", "difficult", RESP, "ph"),
   "Review the day 4 data. Which actions should the nurse take?",
   [("Hold the next tobramycin dose and notify the provider and pharmacy", True,
     "A high trough with a rising creatinine shows accumulation and kidney injury."),
    ("Report the ringing in the ears", True, "Tinnitus is an early sign of aminoglycoside ototoxicity."),
    ("Draw future troughs just before a dose and peaks 30 minutes after an infusion ends", True,
     "Correct timing makes the levels meaningful."),
    ("Give pancreatic enzymes with every meal and snack", True, "Enzymes work only when taken with food."),
    ("Give the tobramycin as scheduled and recheck in 3 days", False, "Continues the toxicity."),
    ("Encourage the client to limit fluids to reduce coughing", False, "Fluids thin secretions and protect the kidneys.")],
   "Aminoglycosides need trough and peak monitoring; a high trough, rising creatinine or tinnitus means holding the "
   "dose.",
   "Take action for aminoglycoside toxicity.",
   ex=[flow("Day 4", ["Day 1", "Day 4"], [
       ("Tobramycin trough", ["-", "3.2 mcg/mL (goal below 1)"]),
       ("Creatinine", ["0.8 mg/dL", "1.4 mg/dL"]),
       ("Client report", ["-", "\"My ears are ringing.\""]),
       ("Enzymes", ["Given with breakfast only", "Given with breakfast only"])], id="cf-day4")]),

matrix_mc(M(PA, PNEU, "evaluate_outcomes", "difficult", RESP, "ms ph"),
   "Review the findings on day 10. For each finding, indicate whether it shows improvement or needs follow-up.",
   ["Shows improvement", "Needs follow-up"],
   [("FEV1 61% of predicted", 0, "Lung function is recovering."),
    ("Sputum lighter in color and easier to clear", 0, "Infection and clearance are improving."),
    ("Formed stools; weight up 1 kg", 0, "Enzymes with meals are working."),
    ("Glucose 140 to 190 mg/dL on insulin", 0, "CF-related diabetes is managed with insulin."),
    ("\"I'll take the enzyme capsules once a day with breakfast.\"", 1, "Enzymes are needed with every meal and snack."),
    ("Creatinine still 1.3 mg/dL after tobramycin was changed", 1, "Kidney injury has not resolved; follow up.")],
   "Lungs, digestion and glucose have improved; the enzyme plan and kidney function need follow-up.",
   "Evaluate outcomes of a CF exacerbation.",
   ex=[flow("Day 10", ["Day 10"], [
       ("FEV1", ["61% of predicted"]),
       ("Sputum", ["Lighter, easier to clear"]),
       ("Stools and weight", ["Formed; up 1 kg"]),
       ("Glucose", ["140 to 190 mg/dL on insulin"]),
       ("Creatinine", ["1.3 mg/dL"]),
       ("Client statement", ["\"I'll take the enzyme capsules once a day with breakfast.\""])], id="cf-day10")]),
]

# ================================================================================================
# Case 2: Aspiration pneumonia after a stroke
# ================================================================================================
asp_tabs = [
    ("asp-notes", "Nurses' notes", [note("Stroke unit, day 2",
        "A 76-year-old client had a left hemisphere ischemic stroke 2 days ago, with right-sided weakness and slurred "
        "speech. The bedside swallow screen has not been done yet; the chart says NPO. At 0800 a UAP fed the client "
        "breakfast with thin coffee, saying the client \"looked hungry.\" The client coughed during the meal and the "
        "voice sounded wet afterward. At 1400 the client is breathless.", id="asp-note")]),
    ("asp-vitals", "Vital signs", [flow("Vital signs", ["0700", "1400"], [
        ("Temperature", ["36.9 C (98.4 F)", "38.4 C (101.1 F)"]),
        ("Respirations", ["18/min", "26/min"]),
        ("SpO2", ["96% on room air", "89% on room air"]),
        ("Breath sounds", ["Clear", "Crackles at the right base"])], id="asp-flow")]),
]

asp_qs = [
hl_text(M(PA, PNEU, "recognize_cues", "moderate", RESP, "ms"),
   "Highlight the findings that suggest aspiration.",
   ["The client has ",
    ("slurred speech after a stroke", True, "Dysarthria often comes with dysphagia."),
    " and ",
    ("right-sided weakness", False, "Expected after a left hemisphere stroke; not itself a sign of aspiration."),
    ". The client was fed ",
    ("thin coffee before a swallow screen", True, "Thin liquids are the most easily aspirated."),
    ", ",
    ("coughed during the meal", True, "Coughing while eating is a sign of aspiration."),
    " and had ",
    ("a wet-sounding voice afterward", True, "A wet voice means fluid near the vocal cords."),
    ". Now there are ",
    ("crackles at the right base", True, "Aspirated material usually goes to the right lower lobe."),
    "."],
   "Dysphagia signs during feeding followed by fever and right basal crackles point to aspiration.",
   "Recognize cues of aspiration."),

cloze(M(PA, PNEU, "analyze_cues", "difficult", RESP, "ms"),
   "Complete the sentences by choosing from the lists of options.",
   "Aspirated material most often reaches the {{b1}} because {{b2}}. Fever and crackles 6 hours after the meal "
   "suggest {{b3}}.",
   [[("right lower lobe", True, "The usual site in an upright client."),
     ("left upper lobe", False, "Less common."),
     ("right upper lobe", False, "More likely when the client is lying flat."),
     ("left lower lobe", False, "Less common than the right.")],
    [("the right main bronchus is wider and more vertical", True, "Gravity carries material down the right side."),
     ("the left lung is smaller", False, "Not the reason."),
     ("the stroke affected the right side", False, "Anatomy, not the weak side, decides this."),
     ("the client coughed", False, "Not the reason.")],
    [("aspiration pneumonitis that may become pneumonia", True, "Early chemical injury; bacterial infection may follow."),
     ("a new stroke", False, "No new neurologic signs."),
     ("heart failure", False, "Not supported."),
     ("an allergic reaction to coffee", False, "Not supported.")]],
   "The right lower lobe is the usual site; early fever and crackles after aspiration suggest pneumonitis.",
   "Analyze aspiration findings."),

matrix_mc(M(PA, PNEU, "prioritize_hypotheses", "difficult", RESP, "ms"),
   "For each risk, indicate its priority now.",
   ["High priority", "Lower priority"],
   [("Further aspiration with every oral intake", 0, "Until swallowing is assessed, every meal risks more aspiration."),
    ("Respiratory failure", 0, "SpO2 89% with tachypnea."),
    ("Pressure injury", 1, "A risk to plan for, but not the immediate threat."),
    ("Depression after stroke", 1, "Important later.")],
   "The priorities are preventing further aspiration and supporting breathing.",
   "Prioritize risks after aspiration."),

matrix_mc(M(PA, STROKE, "generate_solutions", "difficult", RESP, "ms"),
   "For each intervention, indicate whether it is indicated or contraindicated.",
   ["Indicated", "Contraindicated"],
   [("Keep NPO until the swallow screen and speech therapy assessment", 0, "Prevents further aspiration."),
    ("Head of the bed at 30 to 45 degrees at all times", 0, "Reduces aspiration."),
    ("Oral care with a toothbrush at least twice daily", 0, "Reduces oral bacteria that cause pneumonia."),
    ("Give oral medications crushed in applesauce until the screen is done", 1, "Any oral intake is unsafe before screening."),
    ("Oxygen to keep SpO2 at 92% or higher", 0, "Corrects hypoxemia."),
    ("Antibiotics covering oral flora if pneumonia develops, as prescribed", 0, "Treats aspiration pneumonia.")],
   "Stop oral intake, position upright, clean the mouth, give oxygen, and treat infection if it develops.",
   "Generate solutions after aspiration.",
   ex=[note("Orders, 1430", "Chest x-ray: right lower lobe infiltrate. Speech therapy consult. Ampicillin-sulbactam IV.",
       id="asp-orders")]),

sata(M(MOC, DELEG, "take_action", "difficult", RESP, "mgmt"),
   "The nurse reviews how the client was fed. Which actions should the nurse take?",
   [("Place an NPO sign at the bedside and on the door", True, "Makes the restriction visible to every team member."),
    ("Tell the UAP the client's NPO status and why it matters", True, "Clear communication prevents a repeat."),
    ("Include the swallow status in every handoff", True, "Handoff carries safety information."),
    ("Report the event through the incident system", True, "Supports a system review."),
    ("Remove the UAP from the assignment without discussion", False, "Feedback and teaching come first."),
    ("Let the UAP give sips of water only", False, "Thin water is also unsafe before screening.")],
   "Make NPO status visible, communicate it in assignments and handoffs, and report the event.",
   "Take action to prevent a repeat feeding error.",
   ex=[note("1500", "The UAP says, \"Nobody told me the client was NPO.\"", id="asp-1500")]),

matrix_mc(M(BCC, NUTR, "evaluate_outcomes", "difficult", RESP, "ms fund"),
   "Speech therapy recommends a minced diet with mildly thick liquids. The nurse watches lunch. For each "
   "observation, indicate whether it shows safe swallowing practice or needs correction.",
   ["Safe practice", "Needs correction"],
   [("The client sits upright at 90 degrees", 0, "Upright posture protects the airway."),
    ("The client tucks the chin when swallowing", 0, "Narrows the airway entrance."),
    ("Food is placed on the left (unaffected) side of the mouth", 0, "Better control on the stronger side."),
    ("A family member uses a straw for the thickened liquid", 1, "Straws speed liquid into the throat; use a cup or spoon."),
    ("The client lies down right after the meal", 1, "Stay upright 30 minutes after eating."),
    ("Small bites, with a check that each one is swallowed", 0, "Prevents pocketing and aspiration.")],
   "Upright posture, chin tuck, small bites on the strong side and staying upright after meals keep swallowing safe.",
   "Evaluate safe swallowing practice after a stroke.",
   ex=[note("Speech therapy plan, day 4",
       "Minced and moist diet, mildly thick liquids by cup, chin tuck, upright 90 degrees, supervision at meals.",
       id="asp-slp")]),
]

# ================================================================================================
# Case 3: Influenza outbreak in long-term care
# ================================================================================================
flu_tabs = [
    ("flu-notes", "Nurses' notes", [note("Long-term care unit, Tuesday 0700",
        "The RN is the charge nurse on a 30-bed long-term care unit. Since Sunday, three residents have developed "
        "fever, cough and muscle aches. Rapid influenza A tests are positive for all three. About 60% of staff are "
        "vaccinated this season.", id="flu-note")]),
    ("flu-residents", "Residents", [table("Residents with symptoms", ["Findings at 0700"], [
        ("Resident A, 88 years, heart failure", ["Temperature 38.9 C (102 F), RR 28/min, SpO2 88%, new confusion; symptoms began Sunday"]),
        ("Resident B, 79 years, diabetes", ["Temperature 38.2 C (100.8 F), RR 20/min, SpO2 95%, eating; symptoms began Monday"]),
        ("Resident C, 91 years, kidney disease (creatinine clearance 25 mL/min)", ["Temperature 38.0 C (100.4 F), RR 18/min, SpO2 96%; symptoms began this morning"])],
        id="flu-table")]),
]

flu_qs = [
hl_table(M(PA, PNEU, "recognize_cues", "moderate", RESP, "cdc ms"),
   "Highlight the findings that require immediate follow-up.",
   ["Resident", "Finding"],
   [(["Resident A", "SpO2 88%, RR 28/min"], True, "Hypoxemia and tachypnea suggest influenza pneumonia."),
    (["Resident A", "New confusion"], True, "Delirium in an older adult signals severe illness."),
    (["Resident B", "SpO2 95%, eating"], False, "Stable for now."),
    (["Resident C", "Creatinine clearance 25 mL/min"], True, "Antiviral doses must be adjusted for kidney function."),
    (["Staff", "40% unvaccinated"], True, "Unvaccinated staff can spread and catch influenza."),
    (["Resident C", "Temperature 38.0 C (100.4 F)"], False, "Low-grade fever expected with early influenza.")],
   "Severe illness in Resident A, dosing for kidney disease and unvaccinated staff need follow-up.",
   "Recognize cues in an influenza outbreak."),

matrix_mc(M(PPT, ABX, "analyze_cues", "difficult", RESP, "cdc ph"),
   "For each person, indicate what oseltamivir use is appropriate.",
   ["Treatment dose", "Prevention (prophylaxis) dose", "Not needed"],
   [("Resident A, symptomatic for 2 days", 0, "Treat high-risk residents even after 48 hours."),
    ("Resident C, symptomatic today", 0, "Treat at once, at a kidney-adjusted dose."),
    ("An asymptomatic resident on the same unit", 1, "In a long-term care outbreak, all residents receive prophylaxis."),
    ("A resident on another unit with no cases", 2, "Not exposed."),
    ("An unvaccinated staff member without symptoms", 1, "Prophylaxis is offered to unvaccinated staff, and vaccination as well.")],
   "In a long-term care outbreak, treat everyone with symptoms and give prophylaxis to all residents on the unit and "
   "unvaccinated staff.",
   "Analyze antiviral use in an influenza outbreak."),

mc(M(MOC, PRIORITY, "prioritize_hypotheses", "difficult", RESP, "mgmt"),
   "Which resident should the charge nurse assess first?",
   [("Resident A, with SpO2 88%, respirations 28/min and new confusion", True,
     "Signs of hypoxemic respiratory failure and delirium need assessment and likely transfer."),
    ("Resident B, with a temperature of 38.2 C (100.8 F) and diabetes that may be destabilized", False,
     "Needs glucose checks, but is stable."),
    ("Resident C, whose antiviral dose needs adjusting for kidney disease today", False,
     "Important, but not the most urgent."),
    ("A UAP who reports a sore throat and wants to know whether to finish the shift", False,
     "Handled promptly by sending the UAP home, but the hypoxemic resident comes first.")],
   "The resident with hypoxemia, tachypnea and new confusion is the most unstable.",
   "Prioritize residents in an influenza outbreak."),

grouping(M(SIC, TBP, "generate_solutions", "difficult", RESP, "cdc"),
   "For each area of the outbreak plan, select the measures the charge nurse should put in place.",
   [("Residents", [
       ("Droplet precautions for residents with symptoms", True, "Influenza spreads by droplets."),
       ("Keep ill residents in their rooms, or cohort them together", True, "Limits spread."),
       ("Hold the weekly group bingo as usual to keep spirits up", False, "Group activities are paused during an outbreak.")]),
    ("Staff", [
       ("Staff with fever or symptoms stay home until 24 hours fever-free without medicine", True, "Prevents staff-to-resident spread."),
       ("Offer vaccination to unvaccinated staff", True, "Still worthwhile during the season."),
       ("Have the same staff care for ill and well residents on one shift", False, "Cohort staff where possible.")]),
    ("Visitors", [
       ("Post signs and screen visitors for symptoms", True, "Keeps ill visitors out."),
       ("Encourage visitors to take residents to the dining room", False, "Increases mixing.")])],
   "Droplet precautions, cohorting, pausing group activities, excluding ill staff and screening visitors control "
   "an outbreak.",
   "Generate an outbreak control plan.",
   ex=[note("Infection control call, 0800", "The infection preventionist confirms an outbreak and asks the charge "
       "nurse to start the facility outbreak plan.", id="flu-ip")]),

sata(M(PPT, ABX, "take_action", "difficult", RESP, "ph"),
   "Review the medication orders. Which actions should the nurse take?",
   [("Clarify Resident C's dose with the provider before giving it", True,
     "With creatinine clearance of 25 mL/min, the treatment dose is reduced, about 30 mg once daily."),
    ("Give Resident A's first dose now", True, "Antivirals still help in severe illness after 48 hours."),
    ("Watch for new confusion or unusual behavior after starting oseltamivir", True,
     "Neuropsychiatric effects are reported, though rare."),
    ("Wait for viral culture results before starting treatment", False, "Treatment should not be delayed."),
    ("Stop the antiviral once the fever breaks", False, "The 5-day course is completed.")],
   "Start antivirals promptly, adjust doses for kidney function and watch for adverse effects.",
   "Take action on antiviral orders.",
   ex=[note("Medication orders, 0830",
       "Oseltamivir 75 mg PO twice daily for 5 days for Residents A, B and C. Prophylaxis 75 mg daily for all other "
       "residents for at least 2 weeks.", id="flu-meds")]),

matrix_mc(M(SIC, HAI, "evaluate_outcomes", "difficult", RESP, "cdc"),
   "Review the findings 5 days later. For each finding, indicate whether it shows the outbreak plan is working or "
   "needs follow-up.",
   ["Plan is working", "Needs follow-up"],
   [("No new resident cases for 3 days", 0, "Transmission has slowed."),
    ("Resident A transferred to hospital and improving", 0, "Severe illness was escalated appropriately."),
    ("A nurse with a cough worked a full shift on day 4", 1, "Symptomatic staff must stay home."),
    ("Visitor screening log complete each day", 0, "The control measure is in place."),
    ("Two residents on prophylaxis missed doses", 1, "Prophylaxis must continue as prescribed."),
    ("Staff vaccination rate now 85%", 0, "More staff protected.")],
   "Falling new cases show the plan works; staff working while ill and missed prophylaxis need correcting.",
   "Evaluate outbreak control.",
   ex=[flow("Day 5", ["Day 5"], [
       ("New resident cases", ["None for 3 days"]),
       ("Resident A", ["In hospital, improving"]),
       ("Staff", ["A nurse with a cough worked on day 4; vaccination 85%"]),
       ("Visitor screening", ["Complete daily"]),
       ("Prophylaxis", ["Two residents missed doses"])], id="flu-day5")]),
]

# ================================================================================================
# Case 4: Malignant pleural effusion and goals of care
# ================================================================================================
eff_tabs = [
    ("eff-notes", "Nurses' notes", [note("Oncology unit",
        "A 68-year-old client with metastatic lung cancer has had worsening shortness of breath for a week and now "
        "cannot lie flat. Breath sounds are absent over the lower two-thirds of the left chest, with dullness to "
        "percussion and reduced tactile fremitus. The trachea is shifted slightly to the right. Chest x-ray: large "
        "left pleural effusion. The client says, \"I'm tired of hospitals. I want to be at home.\"",
        id="eff-note")]),
    ("eff-vitals", "Vital signs and labs", [table("Vital signs and labs", ["Admission"], [
        ("Respirations", ["28/min"]),
        ("SpO2", ["89% on room air"]),
        ("Heart rate", ["108/min"]),
        ("Platelets", ["182,000/mm3"]),
        ("INR", ["1.1"])], id="eff-table")]),
]

eff_qs = [
hl_table(M(PA, CANCER, "recognize_cues", "moderate", RESP, "ms"),
   "Highlight the findings that are consistent with a large pleural effusion.",
   ["Finding", "Value"],
   [(["Breath sounds", "Absent over the lower left chest"], True, "Fluid blocks transmission of breath sounds."),
    (["Percussion", "Dull"], True, "Fluid is dull; air is resonant or hyperresonant."),
    (["Tactile fremitus", "Reduced"], True, "Fluid dampens vibration."),
    (["Trachea", "Shifted to the right"], True, "A large effusion pushes the mediastinum away."),
    (["Platelets and INR", "182,000/mm3; 1.1"], False, "Normal; they make thoracentesis safe rather than showing the effusion."),
    (["Statement", "\"I want to be at home.\""], False, "Important for goals of care, not a sign of effusion.")],
   "Absent breath sounds, dullness, reduced fremitus and a shift away from the affected side mark a large "
   "effusion.",
   "Recognize cues of pleural effusion."),

matrix_mr(M(PA, CANCER, "analyze_cues", "difficult", RESP, "ms"),
   "For each finding, indicate the conditions it is consistent with. Each row may have more than one answer.",
   ["Pleural effusion", "Pneumothorax", "Lobar consolidation"],
   [("Reduced or absent breath sounds", {0, 1}, "Both fluid and air reduce breath sounds."),
    ("Dullness to percussion", {0, 2}, "Fluid and solid lung are dull."),
    ("Hyperresonance to percussion", {1}, "Air in the pleural space."),
    ("Increased tactile fremitus", {2}, "Consolidated lung transmits vibration better."),
    ("Bronchial breath sounds over the area", {2}, "Heard over consolidation.")],
   "Percussion and fremitus separate fluid, air and consolidation.",
   "Analyze chest findings."),

cloze(M(PA, CANCER, "prioritize_hypotheses", "difficult", RESP, "ms"),
   "Complete the sentences by choosing from the lists of options.",
   "The priority is to relieve {{b1}} with {{b2}}. Because the effusion is likely to recur, the team should also "
   "discuss {{b3}}.",
   [[("dyspnea from lung compression", True, "The effusion compresses the lung and causes hypoxemia."),
     ("pain", False, "Not the main complaint."),
     ("infection", False, "Not supported."),
     ("anxiety alone", False, "The dyspnea has a physical cause.")],
    [("therapeutic thoracentesis", True, "Draining the fluid quickly re-expands the lung."),
     ("a diuretic", False, "Malignant effusions do not respond to diuretics."),
     ("bronchodilators", False, "Not the problem."),
     ("chest physiotherapy", False, "Will not remove fluid.")],
    [("goals of care and options such as an indwelling pleural catheter", True,
      "A catheter allows drainage at home, which matches the client's wish."),
     ("repeat thoracentesis every few days in hospital", False, "Contrary to the client's stated wish."),
     ("withholding all treatment", False, "Symptom relief is still important."),
     ("a lung transplant", False, "Not appropriate in metastatic cancer.")]],
   "Relieve dyspnea now with thoracentesis and plan care that matches the client's goal of being home.",
   "Prioritize care for a malignant effusion."),

sata(M(RRP, RESP_DX, "generate_solutions", "difficult", RESP, "ms"),
   "Which actions should the nurse plan for the thoracentesis?",
   [("Confirm that informed consent is signed", True, "An invasive procedure."),
    ("Sit the client upright, leaning forward over a bedside table", True, "Spreads the ribs and pools fluid at the base."),
    ("Tell the client not to cough, talk or move during needle insertion", True, "Prevents lung puncture."),
    ("Monitor SpO2, breathing and blood pressure throughout", True, "Detects complications early."),
    ("Check that the platelet count and INR are in a safe range", True, "Lowers the risk of bleeding from the needle."),
    ("Position the client flat on the back with arms at the sides", False, "Fluid would not pool for safe access."),
    ("Keep the client NPO for 12 hours before", False, "Not needed for thoracentesis.")],
   "Thoracentesis needs consent, an upright leaning position, stillness during insertion and close monitoring.",
   "Generate a plan for thoracentesis.",
   ex=[note("Procedure plan", "Ultrasound-guided left thoracentesis at the bedside this afternoon.", id="eff-plan")]),

sata(M(RRP, RESP_DX, "take_action", "difficult", RESP, "ms"),
   "During the procedure, after 1,300 mL is drained, the client develops a persistent cough and chest tightness. "
   "Which actions should the nurse take?",
   [("Ask the provider to stop the drainage", True, "These are warning signs of re-expansion pulmonary edema or pneumothorax."),
    ("Check SpO2, breath sounds and blood pressure", True, "Detects complications."),
    ("Give oxygen as needed to keep SpO2 at 92% or higher", True, "Supports oxygenation."),
    ("Prepare for a chest x-ray after the procedure", True, "Checks for pneumothorax and re-expansion edema."),
    ("Encourage the client to cough hard to clear the lungs", False, "Coughing during drainage risks lung puncture."),
    ("Continue draining until the bottle is full", False, "Removing too much fluid quickly causes re-expansion edema.")],
   "Cough and chest tightness during large-volume drainage mean stopping, assessing and imaging.",
   "Take action for complications of thoracentesis.",
   ex=[flow("During thoracentesis", ["Start", "After 1,300 mL"], [
       ("SpO2", ["89%", "86%"]),
       ("Client", ["Breathing easier", "Persistent cough, chest tightness"]),
       ("Blood pressure", ["126/76 mm Hg", "118/72 mm Hg"])], id="eff-proc")]),

matrix_mc(M(MOC, ADVANCE, "evaluate_outcomes", "difficult", RESP, "mgmt psych"),
   "The next day, the client is comfortable and discusses the future with the nurse and family. For each response, "
   "indicate whether it supports the client's goals or needs correction.",
   ["Supports the client's goals", "Needs correction"],
   [("The nurse asks the client what matters most in the time ahead", 0, "Open questions start a goals-of-care conversation."),
    ("The nurse arranges a palliative care consult", 0, "Specialist symptom and goals support."),
    ("The client's son says, \"We'll decide for Dad; he gets too upset.\"", 1,
     "The capable client makes their own decisions; the nurse supports the client's voice."),
    ("The team plans an indwelling pleural catheter with home drainage", 0, "Matches the wish to be at home."),
    ("The nurse documents the client's wishes and updates the code status after the provider discusses it", 0,
     "Wishes are recorded, and code status changes need a provider order."),
    ("Low-dose oral morphine is discussed for breathlessness at home", 0, "Opioids relieve dyspnea in advanced cancer.")],
   "Goals-of-care work respects the client's own decisions, documents wishes and plans symptom control at home.",
   "Evaluate goals-of-care conversations.",
   ex=[note("Next day", "SpO2 95% on room air after drainage of 1,300 mL. The client is comfortable and asks about "
       "going home.", id="eff-nextday")]),
]

cases = [
    case("Cystic fibrosis exacerbation", cf_tabs, cf_qs),
    case("Aspiration pneumonia after a stroke", asp_tabs, asp_qs),
    case("Influenza outbreak in long-term care", flu_tabs, flu_qs),
    case("Malignant pleural effusion and goals of care", eff_tabs, eff_qs),
]

BUILT = build(106, "pulmonary-cases-cf-aspiration-flu-effusion",
              "Pulmonary case studies: cystic fibrosis, aspiration pneumonia, an influenza outbreak and malignant "
              "pleural effusion",
              items, cases, case_conversion=True)
