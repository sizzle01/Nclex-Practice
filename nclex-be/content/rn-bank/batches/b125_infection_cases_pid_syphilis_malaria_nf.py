"""Batch 125 (case conversion, chunk 7: immunity, infection, STI, musculoskeletal and trauma): four full NGN case
studies (CASE-MAP.md, book_topics.py).

Cases: pelvic inflammatory disease from gonorrhea and chlamydia with partner treatment and reporting; secondary
syphilis treated with penicillin and a Jarisch-Herxheimer reaction; severe falciparum malaria in a returning traveler
(antiparasitic drugs); necrotizing fasciitis with surgery, vancomycin and clindamycin. Each case walks the six CJMM
steps in order, unfolds with new data in steps 4 to 6, and includes a management of care, safety or health promotion
question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC = "PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC"
STI = "sexually-transmitted-infections"
PARASITE = "parasitic-and-travel-infections"
add_subtopics({
    STI: ("PHI.PA", "Sexually transmitted infections"),
    PARASITE: ("PHI.PA", "Parasitic and travel-related infections"),
})
REPRO, IMMUNE, INTEG = "reproductive", "immune", "integumentary"
ABX = "antimicrobial-therapy"
SKIN = "skin-disorders"
SEPSIS = "shock-and-sepsis"
REPORT = "mandatory-reporting"
CONFID = "confidentiality-and-information-security"
TEACH = "health-teaching-and-behavior-change"
LABS = "laboratory-values-and-monitoring"
CALC = "medication-dose-calculations"
WOUND = "wound-care"
TBP = "transmission-based-precautions"
COLLAB = "collaboration-and-referrals"

items = []

# ================================================================================================
# Case 1: Pelvic inflammatory disease
# ================================================================================================
pid_tabs = [
    ("pid-notes", "Clinic notes", [note("Urgent care",
        "A 22-year-old client has 4 days of lower abdominal pain, fever and a yellow vaginal discharge, and pain with "
        "intercourse. The client has had two partners in 3 months and uses condoms sometimes. Last menstrual period "
        "was 10 days ago.", id="pid-note")]),
    ("pid-data", "Exam and results", [table("Findings", ["Result"], [
        ("Temperature", ["38.7 C"]),
        ("Cervical motion tenderness", ["Present"]),
        ("Adnexal tenderness", ["Both sides, worse on the right"]),
        ("Urine pregnancy test", ["Negative"]),
        ("Nucleic acid test", ["Positive for gonorrhea and chlamydia"]),
        ("Ultrasound", ["5 cm right tubo-ovarian abscess"]),
        ("Vomiting", ["Unable to keep oral fluids down"])], id="pid-table")]),
]

pid_qs = [
hl_table(M(PA, STI, "recognize_cues", "moderate", REPRO, "ms"),
   "Highlight the findings that support pelvic inflammatory disease (PID).",
   ["Finding", "Result"],
   [(["Cervical motion tenderness", "Present"], True, "A key criterion."),
    (["Adnexal tenderness", "Both sides"], True, "A key criterion."),
    (["Temperature", "38.7 C"], True, "Supports PID."),
    (["Nucleic acid test", "Gonorrhea and chlamydia"], True, "The usual causes."),
    (["Pregnancy test", "Negative"], False, "Excludes ectopic pregnancy; does not support PID.")],
   "Pelvic organ tenderness with fever and an STI confirms PID.",
   "Recognize cues of PID."),

matrix_mc(M(PA, STI, "analyze_cues", "difficult", REPRO, "ms"),
   "For each finding, indicate whether it is a reason to admit the client for IV treatment.",
   ["Reason to admit", "Not a reason to admit"],
   [("Tubo-ovarian abscess", 0, "Needs IV antibiotics and possibly drainage."),
    ("Vomiting and unable to take oral medicine", 0, "Cannot take outpatient treatment."),
    ("High fever with severe illness", 0, "Severe PID."),
    ("Two partners in 3 months", 1, "A risk factor, not an admission criterion."),
    ("Age 22", 1, "Not a criterion.")],
   "Abscess, vomiting and severe illness require admission.",
   "Analyze admission criteria for PID."),

cloze(M(PA, STI, "prioritize_hypotheses", "difficult", REPRO, "ms"),
   "Complete the sentence by choosing from the lists of options.",
   "Without prompt treatment, the client is at greatest risk for {{b1}} and later {{b2}}.",
   [[("abscess rupture with sepsis", True, "A surgical emergency."),
     ("a urinary tract infection", False, "Less serious."),
     ("vaginal yeast", False, "Not the risk."),
     ("anemia", False, "Not the risk.")],
    [("infertility and ectopic pregnancy from scarred tubes", True, "Long-term damage."),
     ("cervical cancer next month", False, "Not the main risk."),
     ("menopause", False, "Not related."),
     ("breast cancer", False, "Not related.")]],
   "PID threatens life now and fertility later.",
   "Prioritize PID risks."),

sata(M(PPT, ABX, "generate_solutions", "difficult", REPRO, "ph cdc"),
   "Which orders should the nurse anticipate?",
   [("Ceftriaxone IV", True, "Covers gonorrhea."),
    ("Doxycycline", True, "Covers chlamydia."),
    ("Metronidazole", True, "Anaerobes and abscess."),
    ("HIV and syphilis testing", True, "Co-infections are common."),
    ("A single dose of oral amoxicillin as the whole treatment", False, "Inadequate for PID."),
    ("An IUD placement while admitted for contraception", False, "Not during active PID.")],
   "IV ceftriaxone, doxycycline and metronidazole with STI screening.",
   "Generate the PID treatment plan.",
   ex=[note("Admission", "Admit to gynecology. Interventional radiology to review the abscess.", id="pid-admit")]),

sata(M(MOC, REPORT, "take_action", "difficult", REPRO, "mgmt cdc"),
   "Which actions should the nurse take about the client's partners and reporting?",
   [("Report gonorrhea and chlamydia to the health department", True, "Reportable infections."),
    ("Encourage the client to tell recent partners so they can be treated", True, "Prevents reinfection."),
    ("Offer expedited partner therapy where state law allows", True, "Treats partners without a visit."),
    ("Call the partners and tell them the client's name and diagnosis", False, "Breaches confidentiality."),
    ("Tell the client that partners only need treatment if they have symptoms", False, "Many partners have no symptoms.")],
   "Report, notify and treat partners while protecting confidentiality.",
   "Take action on STI reporting and partner treatment.",
   ex=[note("Day 2", "Fever settling. Abscess drained.", id="pid-day2")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", REPRO, "cdc"),
   "At discharge, for each statement, indicate whether it shows understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"I'll finish 14 days of doxycycline even when I feel better.\"", 0, "Full course."),
    ("\"I'll avoid sex until my partners and I finish treatment.\"", 0, "Prevents reinfection."),
    ("\"I'll take doxycycline with a full glass of water and stay upright.\"", 0, "Prevents esophageal ulcers."),
    ("\"I'll take doxycycline with milk to protect my stomach.\"", 1, "Calcium blocks absorption."),
    ("\"I'll get retested in 3 months.\"", 0, "Reinfection is common."),
    ("\"Birth control pills protect me from STIs.\"", 1, "Only condoms reduce STI risk.")],
   "Full course, abstinence until partners treated, correct doxycycline use and condoms.",
   "Evaluate PID teaching.",
   ex=[note("Day 4", "Afebrile 48 hours. Switching to oral therapy.", id="pid-day4")]),
]

# ================================================================================================
# Case 2: Secondary syphilis
# ================================================================================================
syph_tabs = [
    ("syph-notes", "Clinic notes", [note("Sexual health clinic",
        "A 31-year-old client has a rash on the palms and soles for 2 weeks, patchy hair loss and mild fever. Two "
        "months ago the client had a painless sore on the genitals that healed by itself. The client is allergic "
        "to nothing known.", id="syph-note")]),
    ("syph-data", "Results", [table("Results", ["Result"], [
        ("RPR", ["Reactive, titer 1:128"]),
        ("Treponemal antibody test", ["Positive"]),
        ("HIV test", ["Negative"]),
        ("Rash", ["Copper-colored, not itchy, on the palms and soles"]),
        ("Mouth", ["Gray mucous patches"]),
        ("Neurologic and eye exam", ["Normal"])], id="syph-table")]),
]

syph_qs = [
hl_text(M(PA, STI, "recognize_cues", "moderate", REPRO, "ms cdc"),
   "Highlight the findings that suggest secondary syphilis.",
   ["The client has a ",
    ("rash on the palms and soles", True, "Classic secondary syphilis."),
    ", ",
    ("patchy hair loss", True, "Syphilitic alopecia."),
    " and ",
    ("gray mucous patches in the mouth", True, "Secondary stage."),
    ". Two months ago there was a ",
    ("painless genital sore", True, "Primary chancre."),
    ". The ",
    ("HIV test is negative", False, "Does not point to syphilis."),
    "."],
   "A palm and sole rash after a painless chancre is secondary syphilis.",
   "Recognize cues of secondary syphilis."),

matrix_mc(M(RRP, LABS, "analyze_cues", "difficult", REPRO, "lab cdc"),
   "For each test, indicate what it tells the nurse.",
   ["Screens and tracks treatment response", "Confirms infection and usually stays positive for life"],
   [("RPR titer", 0, "Nontreponemal; titers fall with treatment."),
    ("VDRL", 0, "Nontreponemal."),
    ("Treponemal antibody test", 1, "Confirms past or present infection."),
    ("FTA-ABS", 1, "Treponemal.")],
   "Nontreponemal titers track treatment; treponemal tests confirm.",
   "Analyze syphilis tests."),

cloze(M(PPT, ABX, "prioritize_hypotheses", "difficult", REPRO, "ph cdc"),
   "Complete the sentence by choosing from the lists of options.",
   "The treatment is {{b1}}, and if the client had a penicillin allergy and were pregnant, the plan would be {{b2}}.",
   [[("one dose of benzathine penicillin G 2.4 million units IM", True, "Treats primary, secondary and early latent syphilis."),
     ("oral amoxicillin for 3 days", False, "Not adequate."),
     ("ceftriaxone eye drops", False, "Not a treatment."),
     ("penicillin G procaine with bicillin C-R mixture", False, "Bicillin C-R is not adequate.")],
    [("penicillin desensitization, then penicillin", True, "No alternative is adequate in pregnancy."),
     ("doxycycline", False, "Contraindicated in pregnancy."),
     ("no treatment until after birth", False, "Congenital syphilis risk."),
     ("azithromycin", False, "Resistance; not adequate.")]],
   "Benzathine penicillin; in pregnancy, desensitize and give penicillin.",
   "Prioritize syphilis treatment."),

sata(M(PPT, "medication-administration-technique", "generate_solutions", "difficult", REPRO, "ph"),
   "How should the nurse give benzathine penicillin G?",
   [("Deep IM into the ventrogluteal site", True, "Large muscle for a thick suspension."),
    ("Inject slowly", True, "A thick suspension."),
    ("Observe the client for 30 minutes afterward", True, "Watch for anaphylaxis."),
    ("Give it IV to make it act faster", False, "IV benzathine penicillin can cause cardiac arrest."),
    ("Give it into the deltoid to reduce pain", False, "The volume is too large for the deltoid.")],
   "Deep IM into a large muscle, never IV, with observation.",
   "Generate a safe administration plan.",
   ex=[note("Order", "Benzathine penicillin G 2.4 million units IM once.", id="syph-order")]),

sata(M(PA, STI, "take_action", "difficult", REPRO, "cdc"),
   "Six hours after the injection, review the findings. Which actions should the nurse take?",
   [("Explain that this expected reaction settles within a day", True, "Jarisch-Herxheimer reaction."),
    ("Recommend acetaminophen for fever and aches", True, "Symptom relief."),
    ("Ask the client to call back if it lasts over a day", True, "Safety net."),
    ("Record a new penicillin allergy in the chart", False, "Not an allergy."),
    ("Repeat the penicillin dose because the rash got worse", False, "Not needed.")],
   "Fever and aches within hours of treatment are a Jarisch-Herxheimer reaction.",
   "Take action on a Jarisch-Herxheimer reaction.",
   ex=[note("Phone call, 6 hours", "Temperature 38.4 C, chills, aches; the rash looks redder. No hives, no wheeze, "
       "no swelling.", id="syph-6h")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", REPRO, "cdc"),
   "At follow-up, for each finding or statement, indicate whether it shows the expected outcome or understanding, or "
   "needs follow-up.",
   ["Expected or understanding", "Needs follow-up"],
   [("RPR 1:16 at 6 months (was 1:128)", 0, "A fourfold or greater fall shows response."),
    ("Treponemal test still positive", 0, "Stays positive for life."),
    ("\"My partners from the last year need testing and treatment.\"", 0, "Correct."),
    ("\"Now that I've had syphilis, I can't catch it again.\"", 1, "Reinfection is possible."),
    ("New vision changes and headache", 1, "Possible neurosyphilis or ocular syphilis.")],
   "Falling titer shows response; reinfection and neurologic symptoms need attention.",
   "Evaluate syphilis treatment.",
   ex=[note("6 months", "Rash resolved.", id="syph-6m")]),
]

# ================================================================================================
# Case 3: Severe falciparum malaria
# ================================================================================================
mal_tabs = [
    ("mal-notes", "Emergency notes", [note("Emergency department",
        "A 45-year-old client returned from visiting family in West Africa 10 days ago and did not take malaria "
        "prophylaxis. The client has had cyclic fevers, chills and headache for 3 days and today is drowsy and "
        "confused.", id="mal-note")]),
    ("mal-data", "Results", [table("Results", ["Result"], [
        ("Temperature", ["40.1 C"]),
        ("Blood smear", ["Plasmodium falciparum, 8% of red cells infected"]),
        ("Glucose", ["52 mg/dL"]),
        ("Hemoglobin", ["8.2 g/dL"]),
        ("Platelets", ["48,000/mm3"]),
        ("Creatinine", ["2.3 mg/dL"]),
        ("Urine", ["Dark"]),
        ("G6PD", ["Pending"])], id="mal-table")]),
]

mal_qs = [
hl_table(M(PA, PARASITE, "recognize_cues", "moderate", IMMUNE, "ms cdc"),
   "Highlight the findings that make this severe malaria.",
   ["Finding", "Result"],
   [(["Mental status", "Drowsy, confused"], True, "Cerebral malaria."),
    (["Parasitemia", "8%"], True, "Above 5% is severe."),
    (["Glucose", "52 mg/dL"], True, "Hypoglycemia."),
    (["Creatinine", "2.3 mg/dL"], True, "Kidney injury."),
    (["Platelets", "48,000/mm3"], False, "Common in malaria, not a criterion on its own.")],
   "Confusion, high parasitemia, hypoglycemia and kidney injury define severe malaria.",
   "Recognize cues of severe malaria."),

matrix_mc(M(PA, PARASITE, "analyze_cues", "difficult", IMMUNE, "ms"),
   "For each finding, indicate its cause in falciparum malaria.",
   ["Infected red cells sticking in small vessels", "Red cell destruction", "Parasite use of glucose and quinine-type drugs"],
   [("Confusion and coma", 0, "Sequestration in the brain."),
    ("Anemia and dark urine", 1, "Hemolysis."),
    ("Hypoglycemia", 2, "Parasites consume glucose."),
    ("Kidney injury", 0, "Sequestration and hemoglobin in the tubules.")],
   "Sequestration and hemolysis explain the organ damage.",
   "Analyze severe malaria."),

cloze(M(PA, PARASITE, "prioritize_hypotheses", "difficult", IMMUNE, "ms ph cdc"),
   "Complete the sentences by choosing from the lists of options.",
   "The nurse's first action is {{b1}}. The antiparasitic drug of choice is {{b2}}.",
   [[("treat the hypoglycemia with IV dextrose", True, "Low glucose worsens the confusion and can kill."),
     ("wait for the G6PD result", False, "Not needed for the first drug."),
     ("give oral chloroquine", False, "Resistant and the client cannot swallow safely."),
     ("place in airborne isolation", False, "Malaria is not spread person to person by air.")],
    [("IV artesunate", True, "First-line for severe malaria."),
     ("oral primaquine", False, "For liver stages of other species, after G6PD testing."),
     ("doxycycline alone", False, "Too slow alone."),
     ("ivermectin", False, "Not for malaria.")]],
   "Correct the glucose, then start IV artesunate.",
   "Prioritize severe malaria care."),

sata(M(PPT, ABX, "generate_solutions", "difficult", IMMUNE, "ph cdc"),
   "Which parts of the plan should the nurse include?",
   [("Glucose checks every 1 to 2 hours", True, "Recurrent hypoglycemia."),
    ("Daily blood smears to track parasitemia", True, "Response."),
    ("Seizure precautions", True, "Cerebral malaria."),
    ("Careful fluid balance", True, "Risk of pulmonary edema."),
    ("Large fluid boluses to flush the kidneys", False, "Can cause pulmonary edema in severe malaria."),
    ("Standard precautions only, no isolation", True, "Not spread person to person except by blood.")],
   "Glucose, smears, seizure safety and careful fluids.",
   "Generate the severe malaria plan.",
   ex=[note("Orders", "Artesunate IV at 0, 12 and 24 hours, then daily. ICU admission.", id="mal-orders")]),

calc(M(PPT, CALC, "take_action", "difficult", IMMUNE, "ph"),
   "Artesunate 2.4 mg/kg IV is prescribed. The client weighs 70 kg. How many mg should the nurse give? Record your "
   "answer as a whole number.",
   "mg", 0, "Dose", 168,
   "2.4 mg x 70 kg = 168 mg.",
   "Calculate a weight-based antiparasitic dose.",
   ex=[note("Weight", "70 kg on the bed scale.", id="mal-weight")]),

matrix_mc(M(PA, PARASITE, "evaluate_outcomes", "difficult", IMMUNE, "ms cdc"),
   "On day 3 and at follow-up, for each finding, indicate whether it shows improvement or needs follow-up.",
   ["Improved", "Needs follow-up"],
   [("Parasitemia below 1% on day 2 and none on day 3", 0, "Responding."),
    ("Alert and oriented", 0, "Cerebral malaria resolved."),
    ("Glucose 98 mg/dL", 0, "Stable."),
    ("Hemoglobin falling to 6.9 g/dL in week 2", 1, "Delayed hemolysis after artesunate."),
    ("\"Next trip I'll take prophylaxis and use bed nets.\"", 0, "Prevention.")],
   "Clinical recovery; watch for delayed hemolysis after artesunate.",
   "Evaluate malaria treatment.",
   ex=[note("Day 3", "Switched to oral artemether-lumefantrine to complete treatment.", id="mal-day3")]),
]

# ================================================================================================
# Case 4: Necrotizing fasciitis
# ================================================================================================
nf_tabs = [
    ("nf-notes", "Emergency notes", [note("Emergency department",
        "A 52-year-old client with type 2 diabetes cut the shin while gardening 2 days ago. The leg is now red and "
        "swollen and the pain is much worse than the skin looks.", id="nf-note")]),
    ("nf-data", "Findings", [flow("Findings", ["Arrival", "3 hours"], [
        ("Pain", ["9/10, out of proportion", "Leg now numb in places"]),
        ("Skin", ["Red, tense, shiny", "Dusky purple, blisters, crackling under the skin"]),
        ("Temperature", ["38.9 C", "39.4 C"]),
        ("Blood pressure", ["104/62", "86/50 mm Hg"]),
        ("Heart rate", ["112", "134/min"]),
        ("Lactate", ["3.1", "4.8 mmol/L"])], id="nf-flow")]),
]

nf_qs = [
hl_table(M(PA, SKIN, "recognize_cues", "moderate", INTEG, "ms"),
   "Highlight the 3-hour findings that suggest a deep, rapidly spreading infection.",
   ["Finding", "3 hours"],
   [(["Pain", "Now numb in places"], True, "Nerve destruction."),
    (["Skin", "Dusky purple, blisters"], True, "Tissue death."),
    (["Crackling under the skin", "Present"], True, "Gas in the tissues."),
    (["Blood pressure", "86/50 mm Hg"], True, "Septic shock."),
    (["Lactate", "4.8 mmol/L"], True, "Hypoperfusion.")],
   "Pain out of proportion, then numbness, blisters, crepitus and shock is necrotizing fasciitis.",
   "Recognize cues of necrotizing fasciitis."),

matrix_mc(M(PA, SKIN, "analyze_cues", "difficult", INTEG, "ms"),
   "For each finding, indicate whether it suggests necrotizing fasciitis or simple cellulitis.",
   ["Necrotizing fasciitis", "Simple cellulitis"],
   [("Pain far worse than the skin appearance", 0, "Classic early sign."),
    ("Crepitus", 0, "Gas."),
    ("Warm, red, tender skin with clear borders and stable vital signs", 1, "Cellulitis."),
    ("Rapid spread over hours with shock", 0, "Necrotizing infection."),
    ("Bullae and skin numbness", 0, "Deep necrosis.")],
   "Pain out of proportion and rapid spread separate the two.",
   "Analyze skin infection severity."),

cloze(M(PA, SEPSIS, "prioritize_hypotheses", "difficult", INTEG, "ms"),
   "Complete the sentence by choosing from the lists of options.",
   "The life-saving treatment is {{b1}}, along with {{b2}}.",
   [[("emergency surgical debridement", True, "Antibiotics cannot reach dead tissue."),
     ("an MRI before any decision", False, "Must not delay surgery."),
     ("elevation and warm compresses", False, "Not enough."),
     ("oral antibiotics at home", False, "Dangerous.")],
    [("broad-spectrum IV antibiotics and fluids", True, "Sepsis care."),
     ("a topical antibiotic cream", False, "Not enough."),
     ("a steroid injection", False, "Harmful."),
     ("an NSAID for pain only", False, "May mask the infection.")]],
   "Surgery now with antibiotics and resuscitation.",
   "Prioritize necrotizing fasciitis."),

matrix_mc(M(PPT, ABX, "generate_solutions", "difficult", INTEG, "ph"),
   "For each antibiotic, indicate its role.",
   ["Covers MRSA", "Stops toxin production", "Covers gram-negative and anaerobic organisms"],
   [("Vancomycin", 0, "MRSA."),
    ("Clindamycin", 1, "Blocks streptococcal toxin production."),
    ("Piperacillin-tazobactam", 2, "Broad gram-negative and anaerobic cover.")],
   "Vancomycin, clindamycin and piperacillin-tazobactam together.",
   "Generate the antibiotic plan.",
   ex=[note("Orders", "To theater now. Vancomycin, piperacillin-tazobactam, clindamycin IV.", id="nf-orders")]),

sata(M(PPT, ABX, "take_action", "difficult", INTEG, "ph"),
   "During the first vancomycin infusion, the client develops flushing and an itchy red rash on the face and "
   "neck. Blood pressure is unchanged. Which actions should the nurse take?",
   [("Stop or slow the infusion", True, "Infusion-rate reaction."),
    ("Notify the provider", True, "Plan the next dose."),
    ("Give diphenhydramine as prescribed", True, "Relieves symptoms."),
    ("Restart later at a slower rate, over at least 60 to 120 minutes, if ordered", True, "Prevents recurrence."),
    ("Record vancomycin as a lifelong anaphylaxis allergy", False, "This is an infusion reaction, not anaphylaxis."),
    ("Speed up the infusion to finish before the rash spreads", False, "Worsens the reaction.")],
   "Flushing with a fast vancomycin infusion is an infusion reaction; slow it down.",
   "Take action for a vancomycin infusion reaction.",
   ex=[note("After surgery", "Extensive debridement of the lower leg fascia. Returns for a second look in 24 hours.", id="nf-postop")]),

matrix_mc(M(SIC, TBP, "evaluate_outcomes", "difficult", INTEG, "ms"),
   "On day 3, wound cultures grow group A streptococcus and MRSA. For each finding or measure, indicate whether it "
   "shows improvement or the correct measure, or needs follow-up.",
   ["Improvement or correct", "Needs follow-up"],
   [("Contact precautions for MRSA", 0, "Correct."),
    ("Lactate 1.4 mmol/L, off vasopressors", 0, "Shock resolved."),
    ("Vancomycin trough or AUC drawn as ordered", 0, "Safe dosing."),
    ("Creatinine rising from 1.0 to 2.2 mg/dL", 1, "Vancomycin and sepsis kidney injury."),
    ("Wound edges pink and bleeding at the second look", 0, "Healthy tissue."),
    ("Blood glucose 320 mg/dL", 1, "Hyperglycemia impairs healing.")],
   "Shock resolved; kidney injury and glucose need attention.",
   "Evaluate necrotizing fasciitis care.",
   ex=[note("Day 3", "Second debridement done. Negative-pressure dressing.", id="nf-day3")]),
]

cases = [
    case("Pelvic inflammatory disease with a tubo-ovarian abscess", pid_tabs, pid_qs),
    case("Secondary syphilis and a Jarisch-Herxheimer reaction", syph_tabs, syph_qs),
    case("Severe falciparum malaria in a returning traveler", mal_tabs, mal_qs),
    case("Necrotizing fasciitis of the leg", nf_tabs, nf_qs),
]

BUILT = build(125, "infection-cases-pid-syphilis-malaria-nf",
              "Infection case studies: PID, syphilis, severe malaria and necrotizing fasciitis",
              items, cases, case_conversion=True)
