"""Batch 151 (chunk 14: second cases for review-book subtopics with one case): four full NGN case studies
(CASE-MAP.md, book_topics.py).

Cases: cryptococcal meningitis in advanced HIV with amphotericin B, flucytosine and fluconazole (antifungal drugs);
trichomoniasis treated with metronidazole and a household pinworm outbreak treated with albendazole (antiparasitic
drugs); statin-associated muscle injury with ezetimibe and a PCSK9 inhibitor (antilipidemics); ACE inhibitor
angioedema with a switch to other antihypertensives (hypertension drugs). Each case walks the six CJMM steps in order,
unfolds with new data in steps 4 to 6, and includes a management of care, safety or health promotion question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC, PSI = ("PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC",
                                          "PSI")
PHARM, IMMUNE, CV = "pharmacology", "immune", "cardiovascular"
ABX = "antimicrobial-therapy"
ADVERSE = "adverse-effects-of-medications"
HIV = "hiv-and-immunodeficiency"
STI = "sexually-transmitted-infections"
PARASITE = "parasitic-and-travel-infections"
HTN = "hypertension-management"
LABS = "laboratory-values-and-monitoring"
LYTES = "electrolyte-imbalances"
TEACH = "health-teaching-and-behavior-change"
MED_SAFETY = "medication-safety-systems"
AIRWAY = "impaired-oxygenation"
REPORT = "mandatory-reporting"

items = []

# ================================================================================================
# Case 1: Cryptococcal meningitis (antifungal drugs)
# ================================================================================================
cm_tabs = [
    ("cm-notes", "Emergency notes", [note("Emergency department",
        "A 41-year-old client with HIV (CD4 38 cells/mm3, not on treatment) has had 2 weeks of worsening headache, "
        "fever and nausea, and today is confused.", id="cm-note")]),
    ("cm-data", "Results", [table("Results", ["Result"], [
        ("Lumbar puncture opening pressure", ["34 cm H2O (high)"]),
        ("CSF cryptococcal antigen", ["Positive"]),
        ("CSF India ink", ["Encapsulated yeast"]),
        ("Creatinine", ["0.9 mg/dL"]),
        ("Potassium", ["3.8 mEq/L"]),
        ("Mental status", ["Oriented to person only"])], id="cm-table")]),
]

cm_qs = [
hl_table(M(PA, HIV, "recognize_cues", "moderate", IMMUNE, "ms ph"),
   "Highlight the findings that require urgent action.",
   ["Finding", "Result"],
   [(["Opening pressure", "34 cm H2O"], True, "Raised ICP causes death and blindness."),
    (["Cryptococcal antigen", "Positive"], True, "Diagnosis."),
    (["Confusion", "Oriented to person only"], True, "Severe disease."),
    (["CD4", "38"], True, "Advanced immunodeficiency."),
    (["Creatinine", "0.9"], False, "Normal baseline before amphotericin.")],
   "Cryptococcal meningitis with high pressure is an emergency.",
   "Recognize cues of cryptococcal meningitis."),

matrix_mc(M(PPT, ABX, "analyze_cues", "difficult", PHARM, "ph"),
   "For each phase of treatment, indicate the drug used.",
   ["Induction (first 2 weeks)", "Consolidation (8 weeks)", "Maintenance (a year or more)"],
   [("Liposomal amphotericin B with flucytosine", 0, "Kills the yeast fast."),
    ("Fluconazole 400 to 800 mg daily", 1, "Step-down."),
    ("Fluconazole 200 mg daily until immune recovery", 2, "Prevents relapse.")],
   "Amphotericin with flucytosine, then fluconazole.",
   "Analyze antifungal treatment phases."),

cloze(M(PA, HIV, "prioritize_hypotheses", "difficult", IMMUNE, "ms"),
   "Complete the sentence by choosing from the lists of options.",
   "Besides antifungals, the client needs {{b1}} to lower the pressure, and antiretrovirals are {{b2}}.",
   [[("repeated lumbar punctures to drain CSF", True, "Removes fluid."),
     ("mannitol as the main treatment", False, "Not effective here."),
     ("steroids", False, "Harmful in cryptococcal meningitis."),
     ("a fluid bolus", False, "Not helpful.")],
    [("delayed about 4 to 6 weeks", True, "Early start raises IRIS and death."),
     ("started today", False, "IRIS risk."),
     ("never started", False, "Needed."),
     ("stopped forever", False, "Needed.")]],
   "Serial LPs; delay ART to avoid IRIS.",
   "Prioritize cryptococcal meningitis care."),

sata(M(PPT, ABX, "generate_solutions", "difficult", PHARM, "ph"),
   "Which parts of the induction plan should the nurse include?",
   [("Saline loading before amphotericin", True, "Protects kidneys."),
    ("Potassium and magnesium checks", True, "Wasting."),
    ("Blood counts during flucytosine", True, "Marrow suppression."),
    ("Premedication for infusion reactions", True, "Comfort."),
    ("Mix amphotericin in 0.9% sodium chloride", False, "Use dextrose 5%; saline precipitates it."),
    ("Give flucytosine at full dose despite kidney injury", False, "Dose adjusts to kidney function.")],
   "Kidney protection, electrolytes, blood counts and premedication.",
   "Generate the induction plan.",
   ex=[note("Orders", "Liposomal amphotericin B and flucytosine. LP daily until pressure is normal.", id="cm-orders")]),

sata(M(PPT, ADVERSE, "take_action", "difficult", PHARM, "ph"),
   "On day 6, review the findings. Which actions should the nurse take?",
   [("Report the labs", True, "Nephrotoxicity."),
    ("Expect K and Mg replacement", True, "Wasting."),
    ("Ask about flucytosine dosing", True, "Cleared by kidneys."),
    ("Stop all antifungal drugs until the kidneys recover", False, "Still needed; adjust."),
    ("Restrict fluids to protect the heart", False, "Worsens kidney injury.")],
   "Amphotericin causes kidney injury and electrolyte loss.",
   "Take action on amphotericin toxicity.",
   ex=[flow("Day 6", ["Day 1", "Day 6"], [
       ("Creatinine", ["0.9", "1.9 mg/dL"]),
       ("Potassium", ["3.8", "2.8 mEq/L"]),
       ("Magnesium", ["2.0", "1.3 mg/dL"]),
       ("WBC", ["4.0", "2.1 x 10^3"])], id="cm-day6")]),

matrix_mc(M(PPT, ABX, "evaluate_outcomes", "difficult", PHARM, "ph"),
   "At discharge on fluconazole, for each statement, indicate whether it shows understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"I'll take fluconazole every day for at least a year.\"", 0, "Maintenance."),
    ("\"I'll start my HIV medicines when the team says, in a few weeks.\"", 0, "Timing."),
    ("\"I'll tell my doctors I'm on fluconazole because it interacts with many drugs.\"", 0, "CYP interactions."),
    ("\"I'll stop fluconazole when the headache goes.\"", 1, "Relapse."),
    ("\"New headache or vision changes mean I call right away.\"", 0, "Relapse or pressure.")],
   "Long-term fluconazole and interaction awareness.",
   "Evaluate antifungal teaching.",
   ex=[note("Day 14", "CSF culture sterile.", id="cm-day14")]),
]

# ================================================================================================
# Case 2: Trichomoniasis and pinworms (antiparasitic drugs)
# ================================================================================================
tr_tabs = [
    ("tr-notes", "Clinic notes", [note("Family clinic",
        "A 28-year-old client has a frothy, yellow-green, foul-smelling vaginal discharge and itching. The client "
        "drinks wine most evenings. The client's 6-year-old child scratches the anal area at night, and the "
        "daycare reports other cases of pinworms.", id="tr-note")]),
    ("tr-data", "Results", [table("Results", ["Result"], [
        ("Wet mount", ["Motile flagellated organisms"]),
        ("Cervix", ["Red spots (strawberry cervix)"]),
        ("Vaginal pH", ["6.0"]),
        ("Pregnancy test", ["Negative"]),
        ("Child's tape test", ["Pinworm eggs seen"])], id="tr-table")]),
]

tr_qs = [
hl_table(M(PA, STI, "recognize_cues", "moderate", PHARM, "ph cdc"),
   "Highlight the findings of trichomoniasis.",
   ["Finding", "Result"],
   [(["Discharge", "Frothy, yellow-green, foul"], True, "Classic."),
    (["Strawberry cervix", "Yes"], True, "Classic."),
    (["Wet mount", "Motile flagellates"], True, "Diagnostic."),
    (["pH", "6.0"], True, "Above 4.5."),
    (["Pregnancy test", "Negative"], False, "Affects the plan, not the diagnosis.")],
   "Frothy discharge with motile organisms is trichomoniasis.",
   "Recognize cues of trichomoniasis."),

matrix_mc(M(PPT, ABX, "analyze_cues", "difficult", PHARM, "ph"),
   "For each antiparasitic drug, indicate its main use.",
   ["Trichomoniasis and giardia", "Pinworm and roundworm", "Scabies and lice", "Malaria"],
   [("Metronidazole", 0, "Protozoa."),
    ("Albendazole or pyrantel", 1, "Worms."),
    ("Permethrin cream", 2, "Ectoparasites."),
    ("Artemether-lumefantrine", 3, "Malaria."),
    ("Ivermectin", 2, "Scabies and lice (also some worms).")],
   "Match antiparasitic drugs to organisms.",
   "Analyze antiparasitic drugs."),

cloze(M(PPT, ABX, "prioritize_hypotheses", "difficult", PHARM, "ph"),
   "Complete the sentence by choosing from the lists of options.",
   "With metronidazole, the client must {{b1}}, because it causes {{b2}}.",
   [[("avoid alcohol during and for 3 days after treatment", True, "Disulfiram-like reaction."),
     ("take it with milk", False, "Not the issue."),
     ("avoid sunlight only", False, "Not the main issue."),
     ("double the dose if a dose is missed", False, "Unsafe.")],
    [("flushing, vomiting and a fast heart rate with alcohol", True, "Disulfiram-like."),
     ("hearing loss", False, "Not typical."),
     ("orange urine", False, "Rifampin."),
     ("tooth staining", False, "Tetracyclines.")]],
   "No alcohol with metronidazole.",
   "Prioritize metronidazole safety."),

sata(M(PPT, ABX, "generate_solutions", "difficult", PHARM, "ph cdc"),
   "Which parts of the plan should the nurse include?",
   [("Metronidazole 500 mg twice daily for 7 days", True, "Treatment."),
    ("Treat sexual partners", True, "Prevents reinfection."),
    ("Albendazole for the child and the whole household", True, "Pinworms spread in families."),
    ("Repeat the pinworm dose in 2 weeks", True, "Kills newly hatched worms."),
    ("Treat the partner only if symptoms appear", False, "Many have none."),
    ("Treat only the child for pinworms", False, "Reinfection from family.")],
   "Treat partners and the household; repeat the worm dose.",
   "Generate the antiparasitic plan.",
   ex=[note("Plan", "Metronidazole for the client and partner. Albendazole for all four household members.", id="tr-plan")]),

sata(M(HPM, TEACH, "take_action", "difficult", PHARM, "peds"),
   "Which hygiene teaching for pinworms should the nurse give?",
   [("Wash hands after the toilet and before eating", True, "Breaks the cycle."),
    ("Keep the child's nails short", True, "Eggs collect under nails."),
    ("Wash bedding and underwear in hot water", True, "Kills eggs."),
    ("Bathe in the morning to remove eggs laid overnight", True, "Removes eggs."),
    ("Keep the child home from school for a month", False, "Not required after treatment."),
    ("Shake out the bedding indoors", False, "Spreads eggs.")],
   "Hand washing, nails, hot washing and morning baths.",
   "Take action with pinworm hygiene teaching.",
   ex=[note("Visit", "Parent asks how to stop the itching coming back.", id="tr-visit")]),

matrix_mc(M(PPT, ABX, "evaluate_outcomes", "difficult", PHARM, "ph"),
   "At follow-up, for each statement or finding, indicate whether it shows understanding or a good outcome, or "
   "needs follow-up.",
   ["Understanding or good outcome", "Needs follow-up"],
   [("\"I had a glass of wine on day 3 and felt flushed and sick.\"", 1, "Disulfiram-like reaction; reinforce."),
    ("Discharge gone", 0, "Treated."),
    ("\"My partner finished treatment too.\"", 0, "Correct."),
    ("Child no longer itching after the second dose", 0, "Treated."),
    ("\"I'll get retested in 3 months.\"", 0, "Reinfection is common.")],
   "Treatment worked; alcohol use needs reinforcement.",
   "Evaluate antiparasitic treatment.",
   ex=[note("3 weeks", "Follow-up visit.", id="tr-3wk")]),
]

# ================================================================================================
# Case 3: Statin muscle injury (antilipidemics)
# ================================================================================================
st_tabs = [
    ("st-notes", "Clinic notes", [note("Cardiology clinic",
        "A 66-year-old client with coronary artery disease takes atorvastatin 80 mg. Gemfibrozil was added 3 weeks "
        "ago for high triglycerides. The client now has aching thighs and weakness climbing stairs and dark urine.",
        id="st-note")]),
    ("st-data", "Results", [table("Results", ["Result"], [
        ("CK", ["9,800 units/L"]),
        ("Creatinine", ["1.8 mg/dL (was 1.0)"]),
        ("Urine dipstick", ["Blood positive, no red cells"]),
        ("LDL", ["68 mg/dL"]),
        ("TSH", ["Normal"]),
        ("Also drinks", ["Grapefruit juice each morning"])], id="st-table")]),
]

st_qs = [
hl_table(M(PPT, ADVERSE, "recognize_cues", "moderate", PHARM, "ph"),
   "Highlight the findings of statin-related muscle injury.",
   ["Finding", "Result"],
   [(["Muscle aches and weakness", "Thighs"], True, "Myopathy."),
    (["CK", "9,800"], True, "Rhabdomyolysis."),
    (["Dark urine with no red cells", "Yes"], True, "Myoglobin."),
    (["Creatinine", "Rising"], True, "Kidney injury."),
    (["LDL", "68"], False, "At goal; not a sign.")],
   "Muscle pain with a high CK and myoglobinuria on a statin is rhabdomyolysis.",
   "Recognize cues of statin myopathy."),

matrix_mc(M(PPT, ADVERSE, "analyze_cues", "difficult", PHARM, "ph"),
   "For each factor, indicate whether it raised this client's risk of statin muscle injury.",
   ["Raised the risk", "Did not raise the risk"],
   [("High-dose atorvastatin", 0, "Dose."),
    ("Gemfibrozil", 0, "A major interaction."),
    ("Grapefruit juice", 0, "Raises atorvastatin levels."),
    ("Age over 65", 0, "Risk."),
    ("Normal thyroid function", 1, "Hypothyroidism would raise risk.")],
   "Fibrates, grapefruit and high doses raise statin toxicity.",
   "Analyze statin interaction risk."),

cloze(M(PPT, ADVERSE, "prioritize_hypotheses", "difficult", PHARM, "ph"),
   "Complete the sentence by choosing from the lists of options.",
   "The nurse should tell the client to {{b1}}, and the client needs {{b2}}.",
   [[("stop the atorvastatin and gemfibrozil now", True, "Remove causes."),
     ("take extra ibuprofen", False, "NSAIDs harm the kidneys."),
     ("exercise to loosen the muscles", False, "Worsens injury."),
     ("continue both and recheck in a month", False, "Dangerous.")],
    [("IV fluids to protect the kidneys", True, "Rhabdomyolysis."),
     ("a higher statin dose", False, "Harmful."),
     ("a potassium supplement", False, "Potassium may be high."),
     ("a diuretic only", False, "Volume first.")]],
   "Stop both and give fluids.",
   "Prioritize statin rhabdomyolysis."),

matrix_mc(M(PPT, ADVERSE, "generate_solutions", "difficult", PHARM, "ph"),
   "After recovery, for each lipid drug, indicate its main teaching point.",
   ["Take at night or as directed; report muscle pain", "Subcutaneous injection every 2 to 4 weeks", "Lowers LDL with few muscle effects; given with food or without", "Flushing; take aspirin 30 minutes before"],
   [("Rosuvastatin low dose", 0, "Statin."),
    ("Evolocumab (PCSK9 inhibitor)", 1, "Injection."),
    ("Ezetimibe", 2, "Absorption blocker."),
    ("Niacin", 3, "Flushing.")],
   "Each lipid drug has its own key point.",
   "Generate lipid drug teaching.",
   ex=[note("Plan", "Rhabdomyolysis resolved. Restart low-dose rosuvastatin plus ezetimibe; consider PCSK9.", id="st-plan")]),

sata(M(SIC, MED_SAFETY, "take_action", "difficult", PHARM, "ph"),
   "Which actions help prevent this interaction from happening again?",
   [("Record the interaction", True, "Alert."),
    ("Pharmacist review", True, "Review."),
    ("Teach to avoid grapefruit", True, "Interaction."),
    ("Tell the client to stop all heart medicines for now", False, "Harmful."),
    ("Record the event as a statin allergy", False, "It is an adverse effect, not an allergy.")],
   "Document interactions and review medicines.",
   "Take action to prevent drug interactions.",
   ex=[note("Day 3", "CK 2,100. Creatinine 1.2.", id="st-day3")]),

matrix_mc(M(PPT, ADVERSE, "evaluate_outcomes", "difficult", PHARM, "ph"),
   "At 3 months, for each finding or statement, indicate whether it shows a good outcome or needs follow-up.",
   ["Good outcome", "Needs follow-up"],
   [("LDL 62 mg/dL on rosuvastatin and ezetimibe", 0, "At goal."),
    ("No muscle pain", 0, "Tolerating."),
    ("ALT normal", 0, "Liver safe."),
    ("\"I switched to grapefruit juice in the evening instead.\"", 1, "Still an interaction for some statins; check."),
    ("\"I'll report muscle pain or dark urine.\"", 0, "Correct.")],
   "At goal and tolerating; grapefruit needs clarifying.",
   "Evaluate lipid therapy.",
   ex=[note("3 months", "Clinic visit.", id="st-3m")]),
]

# ================================================================================================
# Case 4: ACE inhibitor angioedema (hypertension drugs)
# ================================================================================================
ae_tabs = [
    ("ae-notes", "Emergency notes", [note("Emergency department",
        "A 54-year-old client started lisinopril 4 months ago for hypertension. This morning the client woke with "
        "a swollen lower lip and tongue and a hoarse voice. There is no rash or itching.", id="ae-note")]),
    ("ae-data", "Findings", [flow("Findings", ["Arrival", "20 minutes"], [
        ("Lips and tongue", ["Swollen", "Tongue larger, drooling"]),
        ("Voice", ["Hoarse", "Muffled"]),
        ("Breathing", ["No stridor", "Inspiratory stridor"]),
        ("SpO2", ["97%", "92%"]),
        ("Skin", ["No hives", "No hives"]),
        ("Blood pressure", ["148/90", "150/92 mm Hg"])], id="ae-flow")]),
]

ae_qs = [
hl_table(M(PPT, ADVERSE, "recognize_cues", "moderate", PHARM, "ph"),
   "Highlight the 20-minute findings that require immediate action.",
   ["Finding", "20 minutes"],
   [(["Tongue", "Larger, drooling"], True, "Airway swelling."),
    (["Voice", "Muffled"], True, "Laryngeal swelling."),
    (["Stridor", "Inspiratory"], True, "Upper airway obstruction."),
    (["SpO2", "92%"], True, "Falling."),
    (["No hives", "Yes"], False, "Typical of bradykinin angioedema, not a reassurance.")],
   "Swelling of the tongue with stridor is an airway emergency.",
   "Recognize cues of ACE inhibitor angioedema."),

matrix_mc(M(PPT, ADVERSE, "analyze_cues", "difficult", PHARM, "ph"),
   "For each feature, indicate whether it fits ACE inhibitor angioedema or allergic (histamine) angioedema.",
   ["ACE inhibitor (bradykinin) angioedema", "Allergic (histamine) angioedema"],
   [("No hives or itching", 0, "Bradykinin."),
    ("Can occur months or years after starting the drug", 0, "Delayed."),
    ("Responds well to epinephrine and antihistamines", 1, "Histamine."),
    ("Hives and wheezing after a bee sting", 1, "Allergy."),
    ("Higher risk in Black clients", 0, "Known risk.")],
   "Bradykinin angioedema has no hives and responds poorly to antihistamines.",
   "Analyze types of angioedema."),

cloze(M(PA, AIRWAY, "prioritize_hypotheses", "difficult", PHARM, "ms ph"),
   "Complete the sentence by choosing from the lists of options.",
   "The priority is {{b1}}, and lisinopril must be {{b2}}.",
   [[("securing the airway, with a team ready for intubation", True, "Airway first."),
     ("giving oral diphenhydramine and waiting", False, "Too slow and not effective."),
     ("lowering the blood pressure first", False, "Airway first."),
     ("sending for a CT scan", False, "Unsafe.")],
    [("stopped permanently", True, "Never rechallenge."),
     ("restarted at half the dose", False, "Recurrence."),
     ("swapped for another ACE inhibitor", False, "Class effect."),
     ("continued with an antihistamine", False, "Recurrence.")]],
   "Airway first; stop ACE inhibitors for good.",
   "Prioritize angioedema."),

sata(M(PPT, ADVERSE, "generate_solutions", "difficult", PHARM, "ph"),
   "Which treatments might the team use?",
   [("Icatibant", True, "Targets bradykinin."),
    ("Fresh frozen plasma", True, "Contains enzymes that break down bradykinin."),
    ("Epinephrine", True, "Still tried."),
    ("Sit upright", True, "Airway."),
    ("A second lisinopril dose to keep the blood pressure controlled", False, "Worsens."),
    ("Lying flat on the stretcher for comfort", False, "Airway.")],
   "Bradykinin-targeted treatments with airway support.",
   "Generate angioedema treatment.",
   ex=[note("Orders", "Airway team at bedside. Icatibant given. Lisinopril stopped.", id="ae-orders")]),

matrix_mc(M(HPM, HTN, "take_action", "difficult", PHARM, "ph"),
   "For each antihypertensive, indicate its key nursing teaching.",
   ["Watch for ankle swelling", "Monitor potassium and glucose; take in the morning", "Do not stop suddenly", "Rare angioedema; also avoid in pregnancy"],
   [("Amlodipine", 0, "Calcium channel blocker edema."),
    ("Hydrochlorothiazide", 1, "Low potassium, high glucose."),
    ("Metoprolol", 2, "Rebound."),
    ("Losartan", 3, "ARBs rarely cause angioedema.")],
   "Each class has key teaching points.",
   "Take action with antihypertensive teaching.",
   ex=[note("Next day", "Swelling resolved. Provider switching to amlodipine.", id="ae-next")]),

matrix_mc(M(HPM, HTN, "evaluate_outcomes", "difficult", PHARM, "ph"),
   "At discharge, for each statement, indicate whether it shows understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"I'll never take lisinopril or any 'pril' drug again.\"", 0, "Class effect."),
    ("\"I'll wear a medical alert bracelet for ACE inhibitor angioedema.\"", 0, "Safety."),
    ("\"If my lips or tongue swell, I'll call 911.\"", 0, "Correct."),
    ("\"My sister's enalapril is fine for me to borrow.\"", 1, "Same class."),
    ("\"I'll check my blood pressure at home and bring the log.\"", 0, "Monitoring.")],
   "Avoid all ACE inhibitors; know the warning signs.",
   "Evaluate angioedema teaching.",
   ex=[note("Discharge", "Amlodipine 5 mg daily.", id="ae-dc")]),
]

cases = [
    case("Cryptococcal meningitis treated with antifungals", cm_tabs, cm_qs),
    case("Trichomoniasis and a household pinworm outbreak", tr_tabs, tr_qs),
    case("Statin muscle injury from a drug interaction", st_tabs, st_qs),
    case("ACE inhibitor angioedema", ae_tabs, ae_qs),
]

BUILT = build(151, "pharm-cases-crypto-metronidazole-statin-acei",
              "Pharmacology case studies: antifungals for cryptococcal meningitis, antiparasitics, statin muscle "
              "injury and ACE inhibitor angioedema",
              items, cases, case_conversion=True)
