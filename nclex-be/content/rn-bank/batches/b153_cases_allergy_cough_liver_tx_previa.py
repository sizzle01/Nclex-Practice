"""Batch 153 (chunk 14: second cases for review-book subtopics with one case): four full NGN case studies
(CASE-MAP.md, book_topics.py).

Cases: a school-age child with peanut allergy and allergic rhinitis (epinephrine auto-injector, antihistamines,
intranasal steroids, montelukast); cough and mucus in COPD and a child (guaifenesin, acetylcysteine, dextromethorphan,
codeine in children, decongestants with hypertension); liver transplant with CMV infection and tacrolimus; marginal
placenta previa managed expectantly with criteria for home care. Each case walks the six CJMM steps in order, unfolds
with new data in steps 4 to 6, and includes a management of care, safety or health promotion question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC, PSI = ("PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC",
                                          "PSI")
PHARM, PEDS, GI, OB = "pharmacology", "pediatrics", "gastrointestinal", "maternal-newborn"
RESP_MEDS = "respiratory-medications"
TECH = "medication-administration-technique"
IMMUNO = "immunosuppressant-therapy"
COMPLICATIONS = "pregnancy-complications"
ANAPHYLAXIS = "adverse-effects-of-medications"
TEACH = "health-teaching-and-behavior-change"
SCHOOL = "collaboration-and-referrals"
HAI = "healthcare-associated-infection-prevention"
LABS = "laboratory-values-and-monitoring"

items = []

# ================================================================================================
# Case 1: Peanut allergy and allergic rhinitis
# ================================================================================================
pa_tabs = [
    ("pa-notes", "Clinic notes", [note("Allergy clinic",
        "A 9-year-old child had hives and lip swelling after eating a cookie with peanuts last month. The child also "
        "has a runny nose, sneezing and itchy eyes every spring and takes diphenhydramine, which makes the child "
        "sleepy at school.", id="pa-note")]),
    ("pa-data", "Results", [table("Results", ["Result"], [
        ("Peanut IgE", ["High"]),
        ("Asthma", ["Mild, uses albuterol rarely"]),
        ("Weight", ["30 kg"]),
        ("School", ["No action plan on file"]),
        ("Nasal exam", ["Pale, boggy turbinates"])], id="pa-table")]),
]

pa_qs = [
hl_table(M(PPT, RESP_MEDS, "recognize_cues", "moderate", PHARM, "ph peds"),
   "Highlight the findings that raise this child's risk of a severe reaction.",
   ["Finding", "Result"],
   [(["Previous reaction with lip swelling", "Yes"], True, "Systemic reaction."),
    (["Asthma", "Yes"], True, "Raises risk of fatal anaphylaxis."),
    (["No school plan", "Yes"], True, "Delay in treatment."),
    (["Peanut IgE", "High"], True, "Sensitized."),
    (["Spring rhinitis", "Yes"], False, "Separate problem.")],
   "Asthma and lack of a school plan raise the risk of fatal anaphylaxis.",
   "Recognize cues of food allergy risk."),

matrix_mc(M(PPT, RESP_MEDS, "analyze_cues", "difficult", PHARM, "ph peds"),
   "For each allergy drug, indicate its main teaching point.",
   ["First-line for anaphylaxis", "Non-drowsy daily antihistamine", "Best daily rhinitis control; use regularly", "Report mood or behavior changes"],
   [("Epinephrine auto-injector", 0, "Anaphylaxis."),
    ("Cetirizine or loratadine", 1, "Second-generation."),
    ("Fluticasone nasal spray", 2, "Intranasal steroid."),
    ("Montelukast", 3, "Boxed warning for neuropsychiatric effects.")],
   "Epinephrine for anaphylaxis; daily non-sedating drugs for rhinitis.",
   "Analyze allergy drugs."),

cloze(M(PPT, TECH, "prioritize_hypotheses", "difficult", PHARM, "ph peds"),
   "Complete the sentence by choosing from the lists of options.",
   "If the child has trouble breathing after peanuts, the first action is {{b1}}, followed by {{b2}}.",
   [[("epinephrine into the outer thigh", True, "First-line."),
     ("an oral antihistamine and wait", False, "Too slow."),
     ("albuterol only", False, "Does not treat anaphylaxis."),
     ("having the child lie down and sleep", False, "Dangerous.")],
    [("calling 911 and lying the child down with legs raised unless breathing is easier sitting", True, "Emergency care."),
     ("driving home", False, "Delay."),
     ("a second cookie to test", False, "Dangerous."),
     ("a cold drink", False, "Not treatment.")]],
   "Epinephrine first, then 911.",
   "Prioritize anaphylaxis at school."),

sata(M(PPT, TECH, "generate_solutions", "difficult", PHARM, "ph peds"),
   "Which teaching about the epinephrine auto-injector should the nurse give?",
   [("Outer thigh", True, "Technique."),
    ("Hold in place", True, "Full dose."),
    ("Carry two", True, "A second dose may be needed."),
    ("Check expiry and storage", True, "Potency."),
    ("Inject into the buttock or a vein", False, "Wrong site."),
    ("Keep it in the glove box of the car so it is always nearby", False, "Heat damage.")],
   "Thigh, hold, carry two, check storage.",
   "Generate auto-injector teaching.",
   ex=[note("Plan", "Epinephrine 0.3 mg auto-injector x2. Cetirizine daily. Fluticasone nasal spray.", id="pa-plan")]),

sata(M(MOC, SCHOOL, "take_action", "difficult", PHARM, "mgmt peds"),
   "Which actions should the nurse coordinate with the school?",
   [("Give the school nurse a written allergy action plan", True, "Plan."),
    ("Make sure an injector is kept at school and on field trips", True, "Access."),
    ("Teach staff how to use the injector", True, "Readiness."),
    ("Arrange a peanut-free table or other safe seating", True, "Prevention."),
    ("Tell the child to keep the allergy secret", False, "Unsafe."),
    ("Keep the injector locked in the office far from the classroom", False, "Delay.")],
   "Action plan, access, training and prevention.",
   "Take action with school coordination.",
   ex=[note("Visit", "Parent asks what the school should do.", id="pa-school")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", PHARM, "peds ph"),
   "At follow-up, for each statement or finding, indicate whether it shows understanding or progress, or needs "
   "follow-up.",
   ["Understanding or progress", "Needs follow-up"],
   [("\"My child reads food labels with me.\"", 0, "Prevention."),
    ("\"The cetirizine doesn't make my child sleepy in class.\"", 0, "Better."),
    ("\"We used the spray only on bad days.\"", 1, "Needs daily use for best effect."),
    ("\"My child has nightmares since starting montelukast.\"", 1, "Neuropsychiatric effect."),
    ("\"We practiced with the trainer injector.\"", 0, "Readiness.")],
   "Good prevention; spray use and montelukast effects need follow-up.",
   "Evaluate allergy management.",
   ex=[note("3 months", "Spring season.", id="pa-3m")]),
]

# ================================================================================================
# Case 2: Cough and mucus drugs
# ================================================================================================
cc_tabs = [
    ("cc-notes", "Clinic notes", [note("Family clinic",
        "A 68-year-old client with COPD and hypertension has thick sputum and a cough after a cold and has been "
        "taking an over-the-counter decongestant with pseudoephedrine. The client's 3-year-old grandchild has a "
        "cough, and a relative suggested giving the client's codeine cough syrup.", id="cc-note")]),
    ("cc-data", "Findings", [table("Findings", ["Result"], [
        ("Blood pressure", ["172/98 mm Hg (usual 130/80)"]),
        ("Heart rate", ["104/min"]),
        ("Sputum", ["Thick, clear"]),
        ("SpO2", ["92% (baseline)"]),
        ("Sleep", ["Poor; feels jittery"])], id="cc-table")]),
]

cc_qs = [
hl_table(M(PPT, RESP_MEDS, "recognize_cues", "moderate", PHARM, "ph"),
   "Highlight the findings that need action.",
   ["Finding", "Result"],
   [(["Blood pressure", "172/98"], True, "Decongestant raises BP."),
    (["Heart rate", "104/min"], True, "Sympathomimetic."),
    (["Jittery, poor sleep", "Yes"], True, "Stimulant effect."),
    (["Codeine for a 3-year-old", "Suggested"], True, "Contraindicated under 12."),
    (["SpO2", "92%, baseline"], False, "At baseline.")],
   "Decongestants raise BP; codeine is unsafe for young children.",
   "Recognize cues of cough and cold drug risks."),

matrix_mc(M(PPT, RESP_MEDS, "analyze_cues", "difficult", PHARM, "ph"),
   "For each drug, indicate its class.",
   ["Expectorant", "Mucolytic", "Antitussive", "Decongestant"],
   [("Guaifenesin", 0, "Thins secretions to cough up."),
    ("Acetylcysteine nebulized", 1, "Breaks mucus bonds."),
    ("Dextromethorphan", 2, "Suppresses cough."),
    ("Benzonatate", 2, "Numbs stretch receptors."),
    ("Pseudoephedrine", 3, "Vasoconstrictor.")],
   "Know the classes of cough and cold drugs.",
   "Analyze cough and cold drugs."),

cloze(M(PPT, RESP_MEDS, "prioritize_hypotheses", "difficult", PHARM, "ph"),
   "Complete the sentence by choosing from the lists of options.",
   "For this client, the best choice is {{b1}}, and antitussives should {{b2}}.",
   [[("guaifenesin with plenty of fluids, stopping the decongestant", True, "Thins mucus; avoids BP rise."),
     ("more pseudoephedrine", False, "Raises BP."),
     ("codeine", False, "Respiratory depression in COPD."),
     ("an antihistamine to dry secretions", False, "Thickens mucus.")],
    [("be avoided when the cough clears mucus", True, "Productive cough helps COPD."),
     ("always be given at night", False, "Not with productive cough."),
     ("be doubled", False, "Unsafe."),
     ("replace inhalers", False, "No.")]],
   "Expectorant with fluids; avoid suppressing a productive cough.",
   "Prioritize cough treatment in COPD."),

sata(M(PPT, RESP_MEDS, "generate_solutions", "difficult", PHARM, "ph peds"),
   "Which teaching about the grandchild's cough should the nurse give?",
   [("Do not give codeine to children under 12", True, "Fatal respiratory depression."),
    ("Avoid over-the-counter cough and cold medicines under age 4", True, "Risk outweighs benefit."),
    ("Honey is soothing for children over 1 year", True, "Safe remedy."),
    ("Use saline drops and a humidifier", True, "Comfort."),
    ("Give honey to the 3-month-old sibling too", False, "Botulism under 1."),
    ("Give half an adult dose of the cough syrup", False, "Unsafe.")],
   "No codeine or OTC cough medicine for small children; honey over 1.",
   "Generate pediatric cough teaching.",
   ex=[note("Visit", "Client asks what is safe for the grandchildren.", id="cc-grand")]),

sata(M(PPT, TECH, "take_action", "difficult", PHARM, "ph"),
   "In the hospital, the client gets nebulized acetylcysteine for thick mucus. Which actions should the nurse take?",
   [("Give a bronchodilator first", True, "Acetylcysteine can cause bronchospasm."),
    ("Have suction ready", True, "Large volume of secretions."),
    ("Warn about the rotten-egg smell", True, "Expected."),
    ("Stop and report wheezing", True, "Bronchospasm."),
    ("Mix it with the client's other inhalers in one device", False, "Not recommended."),
    ("Expect it to dry up secretions", False, "It loosens them.")],
   "Bronchodilator first, suction ready, expected smell.",
   "Take action with a mucolytic.",
   ex=[note("Admission", "Admitted 3 days later with a COPD exacerbation.", id="cc-admit")]),

matrix_mc(M(PPT, RESP_MEDS, "evaluate_outcomes", "difficult", PHARM, "ph"),
   "At discharge, for each statement, indicate whether it shows understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"I'll read labels and avoid decongestants because of my blood pressure.\"", 0, "Correct."),
    ("\"I'll drink plenty of fluids with guaifenesin.\"", 0, "Correct."),
    ("\"I'll lock away my codeine syrup.\"", 0, "Correct."),
    ("\"Many cold products contain acetaminophen, so I'll check the total dose.\"", 0, "Correct."),
    ("\"A nasal decongestant spray is fine to use for 2 weeks.\"", 1, "Rebound congestion after 3 days.")],
   "Label reading and safe use; nasal sprays only short term.",
   "Evaluate cough and cold drug teaching.",
   ex=[note("Discharge", "BP 134/82.", id="cc-dc")]),
]

# ================================================================================================
# Case 3: Liver transplant with CMV infection
# ================================================================================================
lt_tabs = [
    ("lt-notes", "Transplant notes", [note("Transplant clinic",
        "A 52-year-old client had a liver transplant 10 weeks ago (donor CMV positive, recipient negative) and "
        "takes tacrolimus, mycophenolate and prednisone. Valganciclovir prophylaxis was stopped last week. The "
        "client has fever, diarrhea and tiredness.", id="lt-note")]),
    ("lt-data", "Results", [table("Results", ["Result"], [
        ("Temperature", ["38.5 C"]),
        ("WBC", ["2.4 x 10^3/mm3"]),
        ("Platelets", ["88,000/mm3"]),
        ("ALT", ["140 units/L (was 35)"]),
        ("CMV PCR", ["High viral load"]),
        ("Tacrolimus trough", ["9 ng/mL (target 6 to 10)"])], id="lt-table")]),
]

lt_qs = [
hl_table(M(PPT, IMMUNO, "recognize_cues", "moderate", GI, "ph ms"),
   "Highlight the findings that suggest CMV disease.",
   ["Finding", "Result"],
   [(["Fever", "38.5 C"], True, "Infection."),
    (["Low WBC and platelets", "Yes"], True, "CMV marrow effects."),
    (["Raised ALT", "140"], True, "CMV hepatitis or rejection."),
    (["CMV PCR", "High"], True, "Viremia."),
    (["High-risk match", "Donor positive, recipient negative"], True, "Highest risk.")],
   "A high-risk transplant after stopping prophylaxis with fever and cytopenias is CMV disease.",
   "Recognize cues of CMV after transplant."),

matrix_mc(M(PPT, IMMUNO, "analyze_cues", "difficult", GI, "ph"),
   "For each cause of raised liver enzymes after transplant, indicate how it is confirmed.",
   ["Liver biopsy", "Viral PCR", "Drug level"],
   [("Acute cellular rejection", 0, "Biopsy."),
    ("CMV hepatitis", 1, "PCR and biopsy."),
    ("Tacrolimus toxicity", 2, "Trough level."),
    ("Hepatitis B recurrence", 1, "PCR.")],
   "Rejection, infection and toxicity are separated by different tests.",
   "Analyze liver enzyme rises after transplant."),

cloze(M(PPT, IMMUNO, "prioritize_hypotheses", "difficult", GI, "ph"),
   "Complete the sentence by choosing from the lists of options.",
   "The client needs {{b1}}, and the team may {{b2}}.",
   [[("IV ganciclovir or oral valganciclovir treatment", True, "Treats CMV."),
     ("acyclovir", False, "Not effective for CMV."),
     ("antibiotics only", False, "Viral."),
     ("no treatment", False, "Graft and life at risk.")],
    [("lower the mycophenolate dose while infected", True, "Reduces suppression and marrow effects."),
     ("stop all immunosuppression", False, "Rejection."),
     ("double the tacrolimus", False, "More infection."),
     ("add more prednisone", False, "More infection.")]],
   "Ganciclovir and reduce mycophenolate.",
   "Prioritize CMV treatment."),

sata(M(PPT, IMMUNO, "generate_solutions", "difficult", GI, "ph"),
   "Which parts of the plan should the nurse include during ganciclovir treatment?",
   [("Blood counts twice weekly", True, "Marrow suppression."),
    ("Kidney function for dose adjustment", True, "Renal clearance."),
    ("Weekly CMV PCR", True, "Response."),
    ("Live vaccines to boost immunity", False, "Contraindicated."),
    ("Stop tacrolimus monitoring", False, "Levels still matter.")],
   "Counts, kidneys, safe handling and PCR.",
   "Generate CMV treatment monitoring.",
   ex=[note("Orders", "Ganciclovir IV. Reduce mycophenolate. Biopsy if ALT does not improve.", id="lt-orders")]),

sata(M(SIC, HAI, "take_action", "difficult", GI, "ms"),
   "Which infection prevention teaching should the nurse give?",
   [("Hand hygiene", True, "Prevention."),
    ("Gloves for gardening; no litter boxes", True, "Fungal and toxoplasma."),
    ("Food safety", True, "Food safety."),
    ("Report fever at once", True, "Early treatment."),
    ("Get the live shingles vaccine now to prevent reactivation", False, "Live vaccines avoided."),
    ("Stop prednisone if feeling unwell", False, "Adrenal crisis and rejection.")],
   "Hand hygiene, environmental and food safety, early reporting.",
   "Take action with transplant infection prevention.",
   ex=[note("Day 5", "Fever gone.", id="lt-day5")]),

matrix_mc(M(PPT, IMMUNO, "evaluate_outcomes", "difficult", GI, "ph"),
   "At 3 weeks, for each finding, indicate whether it shows improvement or needs follow-up.",
   ["Improved", "Needs follow-up"],
   [("CMV PCR undetectable", 0, "Cleared."),
    ("ALT 40 units/L", 0, "Normalized."),
    ("WBC 1.2 x 10^3/mm3", 1, "Ganciclovir marrow suppression."),
    ("Afebrile", 0, "Better."),
    ("Tacrolimus 3 ng/mL after a new antibiotic", 1, "Low level; rejection risk.")],
   "CMV cleared; low WBC and low tacrolimus need action.",
   "Evaluate CMV treatment.",
   ex=[note("3 weeks", "Switched to oral valganciclovir.", id="lt-3wk")]),
]

# ================================================================================================
# Case 4: Marginal placenta previa managed expectantly
# ================================================================================================
mp_tabs = [
    ("mp-notes", "Antepartum notes", [note("28 weeks",
        "A 30-year-old client (G2 P1, one previous vaginal birth) at 28 weeks had a small painless bleed this "
        "morning. Ultrasound shows a marginal placenta previa. The bleeding has stopped.", id="mp-note")]),
    ("mp-data", "Findings", [flow("Findings", ["Admission", "Day 3"], [
        ("Bleeding", ["About 50 mL, bright red", "None for 48 hours"]),
        ("Uterus", ["Soft, nontender", "Soft"]),
        ("FHR", ["145/min, moderate variability", "Reactive NST"]),
        ("Hemoglobin", ["11.2", "11.0 g/dL"]),
        ("Blood type", ["A negative", "-"]),
        ("Home", ["Lives 15 minutes away with a partner and a car", "-"])], id="mp-flow")]),
]

mp_qs = [
hl_table(M(PA, COMPLICATIONS, "recognize_cues", "moderate", OB, "ob"),
   "Highlight the findings that support expectant management.",
   ["Finding", "Day 3"],
   [(["No bleeding for 48 hours", "Yes"], True, "Stable."),
    (["Reactive NST", "Yes"], True, "Fetal well-being."),
    (["Stable hemoglobin", "11.0"], True, "No ongoing loss."),
    (["Close to hospital with transport", "Yes"], True, "Home criteria."),
    (["Rh negative", "Yes"], False, "Needs Rho(D) but does not support home care.")],
   "Stable bleeding, fetus and hemoglobin with transport allow expectant management.",
   "Recognize cues for expectant previa management."),

matrix_mc(M(PA, COMPLICATIONS, "analyze_cues", "difficult", OB, "ob"),
   "For each previa type, indicate the likely birth plan.",
   ["Cesarean required", "Vaginal birth may be possible"],
   [("Complete previa covering the cervix", 0, "Cesarean."),
    ("Marginal previa that moves 2 cm or more away by 36 weeks", 1, "Possible."),
    ("Previa with accreta", 0, "Cesarean, often hysterectomy."),
    ("Low-lying placenta at 20 weeks that resolves by 32 weeks", 1, "Common.")],
   "Many low placentas move upward as the uterus grows.",
   "Analyze previa birth planning."),

cloze(M(PA, COMPLICATIONS, "prioritize_hypotheses", "difficult", OB, "ob ph"),
   "Complete the sentence by choosing from the lists of options.",
   "Before discharge, the client needs {{b1}}, and must avoid {{b2}}.",
   [[("Rho(D) immune globulin", True, "Rh negative with bleeding."),
     ("a vaginal exam to measure dilation", False, "Never with previa."),
     ("a tocolytic for home use", False, "Not routine."),
     ("iron infusion", False, "Hemoglobin is normal.")],
    [("intercourse, douching and anything in the vagina", True, "Pelvic rest."),
     ("all walking", False, "Light activity is fine."),
     ("drinking water", False, "Not relevant."),
     ("prenatal vitamins", False, "Continue.")]],
   "Rho(D) and pelvic rest.",
   "Prioritize previa discharge."),

sata(M(PA, COMPLICATIONS, "generate_solutions", "difficult", OB, "ob"),
   "Which parts of the home care plan should the nurse include?",
   [("Return for bleeding", True, "Recurrence."),
    ("Daily kick counts", True, "Fetal well-being."),
    ("Repeat ultrasound", True, "Placental migration."),
    ("Avoid heavy lifting", True, "Reduces bleeding."),
    ("Drive alone 2 hours to visit family this weekend", False, "Far from care."),
    ("Strict bed rest at home", False, "Not proven; raises clot risk.")],
   "Warning signs, kick counts, follow-up ultrasound and activity limits.",
   "Generate previa home care.",
   ex=[note("Day 3", "Discharge planned.", id="mp-day3")]),

sata(M(MOC, "collaboration-and-referrals", "take_action", "difficult", OB, "mgmt ob"),
   "Which actions help the discharge go safely?",
   [("Confirm the client has transport and someone at home", True, "Criteria."),
    ("Give a written plan with the unit phone number", True, "Access."),
    ("Note the previa in the prenatal record for any emergency team", True, "No vaginal exams."),
    ("Book weekly antenatal visits", True, "Follow-up."),
    ("Discharge without telling the outpatient clinic", False, "Continuity."),
    ("Tell the client the previa will definitely resolve", False, "False reassurance.")],
   "Safe home criteria, written plan and communication.",
   "Take action on discharge coordination.",
   ex=[note("Discharge", "Rho(D) given.", id="mp-dc")]),

matrix_mc(M(PA, COMPLICATIONS, "evaluate_outcomes", "difficult", OB, "ob"),
   "At 34 weeks, for each finding, indicate whether it supports the plan or needs prompt action.",
   ["Supports the plan", "Needs prompt action"],
   [("Placenta now 2.5 cm from the cervix", 0, "Migrated."),
    ("No further bleeding", 0, "Stable."),
    ("Reactive NST", 0, "Reassuring."),
    ("A second bleed soaking two pads in an hour", 1, "Readmit."),
    ("Kick counts normal", 0, "Reassuring.")],
   "Placental migration supports a trial of labor; heavy bleeding needs readmission.",
   "Evaluate expectant previa management.",
   ex=[note("34 weeks", "Ultrasound done.", id="mp-34")]),
]

cases = [
    case("Peanut allergy and allergic rhinitis in a school-age child", pa_tabs, pa_qs),
    case("Cough and cold drugs in COPD and in a young child", cc_tabs, cc_qs),
    case("Liver transplant with CMV infection", lt_tabs, lt_qs),
    case("Marginal placenta previa managed expectantly", mp_tabs, mp_qs),
]

BUILT = build(153, "cases-allergy-cough-liver-tx-previa",
              "Case studies: food and seasonal allergy drugs, cough and cold drugs, liver transplant with CMV and "
              "marginal placenta previa",
              items, cases, case_conversion=True)
