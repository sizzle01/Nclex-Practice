"""Batch 143 (case conversion, chunk 12: pediatrics part 1): four full NGN case studies (CASE-MAP.md,
book_topics.py).

Cases: tetralogy of Fallot with a hypercyanotic (tet) spell; Kawasaki disease with IVIG and high-dose aspirin;
infant heart failure from a large VSD with digoxin and furosemide; care after a pediatric cardiac catheterization.
Each case walks the six CJMM steps in order, unfolds with new data in steps 4 to 6, and includes a management of
care, safety or health promotion question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC, PSI = ("PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC",
                                          "PSI")
PEDS = "pediatrics"
CARDIAC = "pediatric-cardiac-disorders"
BASICS = "pediatric-care-basics"
NUTR = "infant-and-child-nutrition"
GROWTH = "infant-growth-and-development"
IMMZ = "immunizations"
CALC = "medication-dose-calculations"
HF_MEDS = "heart-failure-interventions"
POSTPROC = "post-procedure-complications"
TEACH = "health-teaching-and-behavior-change"
COMM = "therapeutic-communication"
MED_SAFETY = "medication-safety-systems"

items = []

# ================================================================================================
# Case 1: Tetralogy of Fallot with a tet spell
# ================================================================================================
tof_tabs = [
    ("tof-notes", "Pediatric unit notes", [note("Pediatric unit",
        "A 5-month-old infant with unrepaired tetralogy of Fallot is admitted for surgery next week. During a "
        "blood draw the infant cries hard and becomes deeply blue and limp.", id="tof-note")]),
    ("tof-data", "Findings", [flow("Findings", ["Before", "During the spell"], [
        ("SpO2", ["84%", "62%"]),
        ("Respiratory rate", ["40", "70/min, deep"]),
        ("Heart rate", ["130", "180/min"]),
        ("Murmur", ["Loud systolic murmur", "Murmur softer"]),
        ("Behavior", ["Calm", "Irritable, then limp"]),
        ("Hematocrit", ["58%", "-"])], id="tof-flow")]),
]

tof_qs = [
hl_table(M(PA, CARDIAC, "recognize_cues", "moderate", PEDS, "peds"),
   "Highlight the findings that show a hypercyanotic (tet) spell.",
   ["Finding", "During the spell"],
   [(["SpO2", "62%"], True, "Severe desaturation."),
    (["Breathing", "Deep, fast"], True, "Hyperpnea."),
    (["Murmur", "Softer"], True, "Less blood crosses the narrowed pulmonary valve."),
    (["Behavior", "Limp"], True, "Hypoxia."),
    (["Hematocrit", "58%"], False, "Chronic polycythemia, not part of the spell.")],
   "A crying infant with ToF who turns deeply blue with a softer murmur is having a tet spell.",
   "Recognize cues of a tet spell."),

matrix_mc(M(PA, CARDIAC, "analyze_cues", "difficult", PEDS, "peds"),
   "For each defect of tetralogy of Fallot, indicate its effect.",
   ["Causes right-to-left shunting", "Obstructs blood flow to the lungs", "Results from pressure overload"],
   [("Ventricular septal defect with an overriding aorta", 0, "Deoxygenated blood enters the aorta."),
    ("Pulmonary stenosis", 1, "Less blood to the lungs."),
    ("Right ventricular hypertrophy", 2, "Pumping against stenosis.")],
   "Pulmonary stenosis drives right-to-left shunting through the VSD.",
   "Analyze tetralogy of Fallot."),

cloze(M(PA, CARDIAC, "prioritize_hypotheses", "difficult", PEDS, "peds"),
   "Complete the sentence by choosing from the lists of options.",
   "The nurse's first action is to {{b1}}, because it {{b2}}.",
   [[("place the infant in a knee-to-chest position", True, "Raises systemic resistance."),
     ("lay the infant flat on the back", False, "Does not help."),
     ("give a sedative IM before positioning", False, "Positioning is first."),
     ("start chest compressions", False, "The heart is beating.")],
    [("increases systemic vascular resistance so more blood goes to the lungs", True, "Reduces shunting."),
     ("lowers the heart rate directly", False, "Not the mechanism."),
     ("opens the VSD", False, "Not the mechanism."),
     ("relaxes the infant only", False, "Not the main effect.")]],
   "Knee-to-chest first.",
   "Prioritize a tet spell."),

sata(M(PPT, "adverse-effects-of-medications", "generate_solutions", "difficult", PEDS, "peds ph"),
   "The spell continues. Which orders should the nurse anticipate?",
   [("Oxygen", True, "Pulmonary vasodilator."),
    ("Morphine", True, "Calms and relaxes the infundibulum."),
    ("IV fluid bolus", True, "Improves preload."),
    ("Propranolol or phenylephrine", True, "Reduces spasm or raises SVR."),
    ("Digoxin IV push to slow the heart rate", False, "Not for a tet spell."),
    ("Albuterol nebulizer to open the airways", False, "Not indicated.")],
   "Oxygen, morphine, fluids and drugs that raise SVR or relax the outflow.",
   "Generate tet spell treatment.",
   ex=[note("2 minutes", "SpO2 still 65% in knee-chest.", id="tof-2min")]),

sata(M(HPM, TEACH, "take_action", "difficult", PEDS, "peds"),
   "Before surgery, which teaching should the nurse give the parents?",
   [("Knees to chest and call 911 for a spell", True, "Home spell plan."),
    ("Prevent dehydration", True, "Polycythemia stroke risk."),
    ("Keep the baby calm", True, "Prevents spells."),
    ("Tell the dentist about the defect", True, "Endocarditis prophylaxis may be needed."),
    ("Let the baby cry it out at night to build strength", False, "Triggers spells."),
    ("Withhold all routine vaccines until after the surgery", False, "Routine vaccines continue.")],
   "Spell plan, hydration, calm care and endocarditis awareness.",
   "Take action with ToF teaching.",
   ex=[note("After the spell", "SpO2 back to 84%. Surgery moved earlier.", id="tof-after")]),

matrix_mc(M(PA, CARDIAC, "evaluate_outcomes", "difficult", PEDS, "peds"),
   "After complete repair, for each finding, indicate whether it is expected or needs follow-up.",
   ["Expected", "Needs follow-up"],
   [("SpO2 97% on room air", 0, "Shunt closed."),
    ("Pink lips and nail beds", 0, "Improved."),
    ("Heart rate 60/min with a new rhythm on the monitor", 1, "Heart block after VSD repair."),
    ("Feeding well and gaining weight", 0, "Recovery."),
    ("Chest tube drainage 8 mL/kg in an hour", 1, "Bleeding.")],
   "Pink and feeding; heart block or bleeding needs follow-up.",
   "Evaluate ToF repair.",
   ex=[note("Post-op day 1", "Complete repair done.", id="tof-pod1")]),
]

# ================================================================================================
# Case 2: Kawasaki disease
# ================================================================================================
kd_tabs = [
    ("kd-notes", "Pediatric unit notes", [note("Admission",
        "A 3-year-old child has had a fever above 39 C for 6 days that does not respond to antipyretics. The child "
        "is very irritable.", id="kd-note")]),
    ("kd-data", "Exam", [table("Exam", ["Finding"], [
        ("Eyes", ["Red without discharge"]),
        ("Mouth", ["Cracked red lips, strawberry tongue"]),
        ("Hands and feet", ["Red and swollen"]),
        ("Rash", ["Widespread, red"]),
        ("Neck", ["One large lymph node, 2 cm"]),
        ("ESR and CRP", ["High"]),
        ("Platelets", ["520,000/mm3"])], id="kd-table")]),
]

kd_qs = [
hl_table(M(PA, CARDIAC, "recognize_cues", "moderate", PEDS, "peds"),
   "Highlight the findings that meet the criteria for Kawasaki disease.",
   ["Finding", "Result"],
   [(["Fever", "6 days"], True, "Required for 5 days or more."),
    (["Eyes", "Red without discharge"], True, "Conjunctivitis."),
    (["Mouth", "Strawberry tongue, cracked lips"], True, "Oral changes."),
    (["Hands and feet", "Swollen, red"], True, "Extremity changes."),
    (["Neck node", "2 cm"], True, "Lymphadenopathy."),
    (["Platelets", "520,000"], False, "Supports, but not a criterion.")],
   "Fever for 5 days with four of five signs is Kawasaki disease.",
   "Recognize cues of Kawasaki disease."),

matrix_mc(M(PA, CARDIAC, "analyze_cues", "difficult", PEDS, "peds"),
   "For each phase of Kawasaki disease, indicate the typical findings.",
   ["Acute phase", "Subacute phase", "Convalescent phase"],
   [("High fever, red eyes, strawberry tongue", 0, "First 1 to 2 weeks."),
    ("Peeling of fingers and toes, very high platelets", 1, "Weeks 2 to 4."),
    ("Highest risk of coronary aneurysm", 1, "Subacute."),
    ("Signs resolve; ESR returns to normal", 2, "6 to 8 weeks.")],
   "Coronary aneurysms are most likely in the subacute phase.",
   "Analyze Kawasaki phases."),

cloze(M(PA, CARDIAC, "prioritize_hypotheses", "difficult", PEDS, "peds ph"),
   "Complete the sentence by choosing from the lists of options.",
   "The main goal of treatment is to prevent {{b1}}, using {{b2}}.",
   [[("coronary artery aneurysms", True, "The major complication."),
     ("seizures", False, "Not the main risk."),
     ("kidney failure", False, "Not the main risk."),
     ("liver failure", False, "Not the main risk.")],
    [("IVIG and high-dose aspirin", True, "Standard."),
     ("acetaminophen alone", False, "Does not prevent aneurysms."),
     ("antibiotics", False, "Not infection."),
     ("steroids alone", False, "Not first-line.")]],
   "IVIG and aspirin prevent coronary aneurysms.",
   "Prioritize Kawasaki treatment."),

calc(M(PPT, CALC, "generate_solutions", "difficult", PEDS, "ph peds"),
   "IVIG 2 g/kg is prescribed. The child weighs 14 kg. How many grams should the nurse prepare? Record your answer "
   "as a whole number.",
   "g", 0, "Dose", 28,
   "2 g x 14 kg = 28 g.",
   "Calculate an IVIG dose.",
   ex=[note("Orders", "IVIG 2 g/kg over 10 to 12 hours. Aspirin 80 to 100 mg/kg/day in 4 doses. Echocardiogram.", id="kd-orders")]),

sata(M(SIC, MED_SAFETY, "take_action", "difficult", PEDS, "ph peds"),
   "During the IVIG infusion, which actions should the nurse take?",
   [("Frequent vital signs", True, "Infusion reactions."),
    ("Start slowly", True, "Reduces reactions."),
    ("Stop for a reaction", True, "Reaction."),
    ("Mix the IVIG with other IV medicines to save lines", False, "Infuse alone."),
    ("Shake the bottle well before hanging it to mix", False, "Denatures protein.")],
   "Slow start, frequent checks and stop for reactions.",
   "Take action during IVIG.",
   ex=[note("IVIG", "Infusion started.", id="kd-ivig")]),

matrix_mc(M(HPM, IMMZ, "evaluate_outcomes", "difficult", PEDS, "peds acip"),
   "At discharge, for each statement by the parents, indicate whether it shows understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"We'll wait 11 months for the MMR and chickenpox vaccines.\"", 0, "IVIG blocks live vaccines."),
    ("\"If our child is exposed to flu or chickenpox while on aspirin, we'll call.\"", 0, "Reye syndrome risk."),
    ("\"We'll keep the echocardiogram appointments.\"", 0, "Aneurysm checks."),
    ("\"We'll give ibuprofen for any fever along with the aspirin.\"", 1, "Ibuprofen blocks aspirin's antiplatelet effect."),
    ("\"The peeling fingers mean the disease is getting worse.\"", 1, "Peeling is an expected subacute sign.")],
   "Vaccine delay, Reye risk and follow-up; avoid ibuprofen with aspirin.",
   "Evaluate Kawasaki teaching.",
   ex=[note("Day 3", "Afebrile. Echo: mild coronary dilation. Low-dose aspirin continued.", id="kd-day3")]),
]

# ================================================================================================
# Case 3: Infant heart failure from a large VSD
# ================================================================================================
vsd_tabs = [
    ("vsd-notes", "Clinic notes", [note("Cardiology clinic",
        "A 2-month-old infant with a large ventricular septal defect sweats and stops during feeds, takes 45 minutes "
        "per bottle and is dropping on the growth chart.", id="vsd-note")]),
    ("vsd-data", "Findings", [table("Findings", ["Result"], [
        ("Respiratory rate", ["68/min at rest"]),
        ("Heart rate", ["172/min"]),
        ("Liver", ["3 cm below the rib margin"]),
        ("Weight", ["Below the 3rd percentile"]),
        ("Lungs", ["Fine crackles"]),
        ("Potassium", ["3.2 mEq/L"])], id="vsd-table")]),
]

vsd_qs = [
hl_table(M(PA, CARDIAC, "recognize_cues", "moderate", PEDS, "peds"),
   "Highlight the findings of heart failure in this infant.",
   ["Finding", "Result"],
   [(["Feeding", "Sweating, tires, 45 minutes"], True, "Early sign in infants."),
    (["Tachypnea", "68/min"], True, "Pulmonary congestion."),
    (["Tachycardia", "172/min"], True, "Compensation."),
    (["Liver", "3 cm below margin"], True, "Hepatomegaly."),
    (["Failure to thrive", "Below 3rd percentile"], True, "Energy demand.")],
   "Feeding difficulty, tachypnea and hepatomegaly are infant heart failure signs.",
   "Recognize cues of infant heart failure."),

matrix_mc(M(PA, CARDIAC, "analyze_cues", "difficult", PEDS, "peds"),
   "For each defect, indicate the direction of shunting.",
   ["Left to right (more blood to lungs)", "Right to left (cyanotic)"],
   [("Ventricular septal defect", 0, "Acyanotic."),
    ("Patent ductus arteriosus", 0, "Acyanotic."),
    ("Tetralogy of Fallot", 1, "Cyanotic."),
    ("Atrial septal defect", 0, "Acyanotic."),
    ("Transposition of the great arteries", 1, "Cyanotic.")],
   "Left-to-right shunts overload the lungs and cause heart failure.",
   "Analyze congenital heart defects."),

cloze(M(HPM, NUTR, "prioritize_hypotheses", "difficult", PEDS, "peds"),
   "Complete the sentence by choosing from the lists of options.",
   "The priority nursing goal is {{b1}}, using {{b2}}.",
   [[("adequate nutrition with less energy use", True, "Growth."),
     ("restricting all fluids", False, "Dehydration."),
     ("increasing activity", False, "More demand."),
     ("starting solid foods now", False, "Too early.")],
    [("small frequent feeds with higher-calorie formula and a soft nipple, limiting feeds to 30 minutes", True, "Less work."),
     ("large feeds every 6 hours", False, "Too tiring."),
     ("feeding flat on the back", False, "Aspiration."),
     ("feeding until the bottle is empty however long it takes", False, "Exhausting.")]],
   "Concentrated calories with short, frequent feeds.",
   "Prioritize infant heart failure care."),

calc(M(PPT, CALC, "generate_solutions", "difficult", PEDS, "ph peds"),
   "Digoxin 10 mcg/kg/day divided twice daily is prescribed. The infant weighs 4 kg. The elixir is 50 mcg/mL. How "
   "many mL should the nurse give per dose? Record your answer to one decimal place.",
   "mL", 1, "Volume per dose", 0.4,
   "10 x 4 = 40 mcg/day; 20 mcg per dose; 20 / 50 = 0.4 mL.",
   "Calculate a pediatric digoxin dose.",
   ex=[note("Orders", "Digoxin 10 mcg/kg/day in 2 doses. Furosemide 1 mg/kg twice daily. Potassium supplement.", id="vsd-orders")]),

sata(M(PPT, HF_MEDS, "take_action", "difficult", PEDS, "ph peds"),
   "Before the morning digoxin dose, the apical pulse is 84/min and the infant vomited twice. Which actions should "
   "the nurse take?",
   [("Hold the dose", True, "Below 90 to 110 in infants."),
    ("Notify the provider", True, "Possible toxicity."),
    ("Check the digoxin and potassium levels as ordered", True, "Low potassium raises toxicity."),
    ("Give the dose and recheck later", False, "Unsafe."),
    ("Repeat the dose because the infant vomited", False, "Never repeat.")],
   "Bradycardia and vomiting suggest digoxin toxicity, worsened by low potassium.",
   "Take action on digoxin toxicity.",
   ex=[note("Day 4", "Potassium 3.0 mEq/L.", id="vsd-day4")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", PEDS, "peds ph"),
   "At discharge, for each statement by the parent, indicate whether it shows understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"I'll give digoxin with an oral syringe into the side of the cheek.\"", 0, "Correct."),
    ("\"If a dose is vomited, I won't repeat it.\"", 0, "Correct."),
    ("\"I'll mix the digoxin in the whole bottle of formula.\"", 1, "Dose lost if the bottle is not finished."),
    ("\"I'll weigh the baby every day on the same scale.\"", 0, "Fluid."),
    ("\"If I miss a dose by more than 4 hours, I'll double the next one.\"", 1, "Never double.")],
   "Safe digoxin use and daily weights.",
   "Evaluate digoxin teaching.",
   ex=[note("Discharge", "Surgical repair planned at 4 to 6 months.", id="vsd-dc")]),
]

# ================================================================================================
# Case 4: After pediatric cardiac catheterization
# ================================================================================================
cath_tabs = [
    ("cath-notes", "Post-procedure notes", [note("Pediatric unit",
        "A 4-year-old child had a cardiac catheterization through the right femoral artery to close an atrial "
        "septal defect. The child returned to the unit 1 hour ago.", id="cath-note")]),
    ("cath-data", "Checks", [flow("Checks", ["30 minutes", "1 hour"], [
        ("Right foot", ["Warm, pink, pulse 2+", "Cool, pale, pulse weak"]),
        ("Left foot", ["Warm, pulse 2+", "Warm, pulse 2+"]),
        ("Dressing", ["Dry", "Small spot"]),
        ("Heart rate", ["100", "104/min"]),
        ("Behavior", ["Sleepy", "Wants to sit up"])], id="cath-flow")]),
]

cath_qs = [
hl_table(M(RRP, POSTPROC, "recognize_cues", "moderate", PEDS, "peds"),
   "Highlight the 1-hour findings that need action.",
   ["Finding", "1 hour"],
   [(["Right foot", "Cool, pale, weak pulse"], True, "Arterial occlusion or spasm."),
    (["Wants to sit up", "Yes"], True, "Must keep the leg straight."),
    (["Dressing", "Small spot"], False, "Monitor."),
    (["Left foot", "Warm"], False, "Normal comparison."),
    (["Heart rate", "104/min"], False, "Normal for age.")],
   "A cool, pulseless limb after femoral catheterization is an emergency.",
   "Recognize cues after cardiac catheterization."),

matrix_mc(M(RRP, POSTPROC, "analyze_cues", "difficult", PEDS, "peds"),
   "For each finding after catheterization, indicate the likely problem.",
   ["Arterial thrombosis", "Bleeding at the site", "Dysrhythmia"],
   [("Cool pale foot with a weak pulse", 0, "Clot."),
    ("Swelling and spreading blood under the dressing", 1, "Bleeding."),
    ("Irregular heart rate on the monitor", 2, "Catheter irritation."),
    ("Falling blood pressure with a rising heart rate", 1, "Blood loss.")],
   "Pulse, site and rhythm checks find complications early.",
   "Analyze catheterization complications."),

cloze(M(RRP, POSTPROC, "prioritize_hypotheses", "difficult", PEDS, "peds"),
   "Complete the sentence by choosing from the lists of options.",
   "The nurse should {{b1}}, and the child is likely to need {{b2}}.",
   [[("notify the provider at once", True, "Limb threatened."),
     ("elevate the leg and recheck in 2 hours", False, "Delay."),
     ("apply a heating pad", False, "Burn risk."),
     ("massage the leg", False, "Harmful.")],
    [("heparin infusion", True, "Treats arterial thrombosis."),
     ("aspirin only", False, "Not enough."),
     ("an antibiotic", False, "Not infection."),
     ("a diuretic", False, "Not relevant.")]],
   "Notify and expect heparin.",
   "Prioritize a cold limb."),

sata(M(RRP, POSTPROC, "generate_solutions", "difficult", PEDS, "peds"),
   "Which parts of the post-catheterization plan should the nurse include?",
   [("Keep the leg straight for 4 to 6 hours as ordered", True, "Prevents bleeding."),
    ("Check pulses, color and temperature below the site often", True, "Circulation."),
    ("Encourage fluids", True, "Clears contrast."),
    ("Watch for low blood sugar in young children who were NPO", True, "Risk."),
    ("Let the child sit up and play right away", False, "Bleeding."),
    ("Remove the pressure dressing at 1 hour", False, "Too early.")],
   "Leg straight, distal checks, fluids and glucose.",
   "Generate post-catheterization care.",
   ex=[note("Plan", "Heparin started. Right foot pulse returned at 3 hours.", id="cath-plan")]),

sata(M(PSI, "coping-and-adjustment", "take_action", "difficult", PEDS, "peds psych"),
   "The 4-year-old is crying and wants to get out of bed. Which actions fit this child's developmental stage?",
   [("Offer simple choices, such as which video to watch", True, "Preschoolers want control."),
    ("Let a parent stay at the bedside", True, "Comfort."),
    ("Use a favorite toy or story as distraction", True, "Effective."),
    ("Explain that staying still is a punishment for crying", False, "Preschoolers may see illness as punishment."),
    ("Give long medical explanations", False, "Too complex.")],
   "Choices, parents and play suit preschoolers.",
   "Take action with developmentally appropriate care.",
   ex=[note("3 hours", "Child restless; parent present.", id="cath-3h")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", PEDS, "peds"),
   "At discharge, for each statement by the parent, indicate whether it shows understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"No tub baths or swimming for a few days.\"", 0, "Correct."),
    ("\"If the site bleeds, I'll press firmly above it and call 911.\"", 0, "Correct."),
    ("\"I'll check that the foot is warm and pink.\"", 0, "Correct."),
    ("\"A little fever for a week is normal.\"", 1, "Report fever."),
    ("\"Rough play like the trampoline is fine tomorrow.\"", 1, "Avoid strenuous activity for a few days.")],
   "Site care and activity limits.",
   "Evaluate post-catheterization teaching.",
   ex=[note("Day 2", "Both feet warm and pink.", id="cath-day2")]),
]

cases = [
    case("Tetralogy of Fallot with a tet spell", tof_tabs, tof_qs),
    case("Kawasaki disease", kd_tabs, kd_qs),
    case("Infant heart failure from a ventricular septal defect", vsd_tabs, vsd_qs),
    case("Care after a pediatric cardiac catheterization", cath_tabs, cath_qs),
]

BUILT = build(143, "peds-cases-tof-kawasaki-vsd-cath",
              "Pediatric case studies: tetralogy of Fallot, Kawasaki disease, infant heart failure and cardiac "
              "catheterization",
              items, cases, case_conversion=True)
