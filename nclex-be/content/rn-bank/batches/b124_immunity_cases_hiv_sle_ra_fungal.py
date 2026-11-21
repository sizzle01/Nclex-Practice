"""Batch 124 (case conversion, chunk 7: immunity, infection, STI, musculoskeletal and trauma): four full NGN case
studies (CASE-MAP.md, book_topics.py).

Cases: advanced HIV with Pneumocystis pneumonia and starting antiretroviral therapy; systemic lupus erythematosus
flare with lupus nephritis; rheumatoid arthritis on methotrexate and a TNF inhibitor with a serious infection;
candidemia treated with an echinocandin and amphotericin B (antifungal drugs). Each case walks the six CJMM steps in
order, unfolds with new data in steps 4 to 6, and includes a management of care, safety or health promotion
question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC = "PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC"
IMMUNE = "immune"
HIV = "hiv-and-immunodeficiency"
AUTO = "autoimmune-disorders"
ARTH = "arthritis-and-gout"
IMMUNO = "immunosuppressant-therapy"
ABX = "antimicrobial-therapy"
LABS = "laboratory-values-and-monitoring"
OXY = "oxygen-therapy-and-devices"
TBP = "transmission-based-precautions"
HAI = "healthcare-associated-infection-prevention"
CONFID = "confidentiality-and-information-security"
TEACH = "health-teaching-and-behavior-change"
SEPSIS = "shock-and-sepsis"
KIDNEY = "kidney-injury-and-disease"
CALC = "medication-dose-calculations"
IMMZ = "immunizations"

items = []

# ================================================================================================
# Case 1: Advanced HIV with Pneumocystis pneumonia
# ================================================================================================
hiv_tabs = [
    ("hiv-notes", "Emergency notes", [note("Emergency department",
        "A 38-year-old client has had 3 weeks of dry cough, fever and breathlessness that is worse on exertion. The "
        "client has lost 7 kg and has white plaques in the mouth. The client tested HIV positive 5 years ago but "
        "never started treatment. The client's sister is at the bedside.", id="hiv-note")]),
    ("hiv-data", "Findings", [table("Findings", ["Result"], [
        ("SpO2", ["89% at rest, 81% after walking 20 m"]),
        ("Respiratory rate", ["28/min"]),
        ("Temperature", ["38.6 C"]),
        ("Chest x-ray", ["Bilateral diffuse ground-glass infiltrates"]),
        ("CD4 count", ["62 cells/mm3"]),
        ("Viral load", ["410,000 copies/mL"]),
        ("PaO2 on room air", ["58 mm Hg"]),
        ("LDH", ["Raised"])], id="hiv-table")]),
]

hiv_qs = [
hl_table(M(PA, HIV, "recognize_cues", "moderate", IMMUNE, "ms"),
   "Highlight the findings that require immediate follow-up.",
   ["Finding", "Result"],
   [(["SpO2", "89% at rest, 81% on walking"], True, "Exertional desaturation is typical of Pneumocystis pneumonia."),
    (["PaO2", "58 mm Hg"], True, "Below 70 means steroids are added."),
    (["CD4", "62 cells/mm3"], True, "Below 200 is AIDS; high opportunistic infection risk."),
    (["Chest x-ray", "Diffuse ground-glass infiltrates"], True, "Pneumocystis pattern."),
    (["Mouth", "White plaques"], True, "Oral candidiasis, a marker of immune failure.")],
   "Hypoxemia with a CD4 below 200 and diffuse infiltrates suggests Pneumocystis pneumonia.",
   "Recognize cues of Pneumocystis pneumonia."),

matrix_mc(M(PA, HIV, "analyze_cues", "difficult", IMMUNE, "ms"),
   "For each CD4 count, indicate the infection the client needs prophylaxis against once the count falls below it.",
   ["Pneumocystis (and toxoplasmosis by antibody status)", "Mycobacterium avium complex in some guidelines", "No prophylaxis needed"],
   [("CD4 below 200 cells/mm3", 0, "Start trimethoprim-sulfamethoxazole."),
    ("CD4 below 100 with positive toxoplasma antibody", 0, "Trimethoprim-sulfamethoxazole covers both."),
    ("CD4 below 50 without starting antiretrovirals", 1, "MAC prophylaxis is only for those not starting ART at once."),
    ("CD4 650 cells/mm3", 2, "Immune function is adequate.")],
   "CD4 thresholds guide which opportunistic infections to prevent.",
   "Analyze CD4-based prophylaxis."),

cloze(M(PPT, ABX, "prioritize_hypotheses", "difficult", IMMUNE, "ph ms"),
   "Complete the sentences by choosing from the lists of options.",
   "The first-line treatment is {{b1}}. Because the PaO2 is below 70 mm Hg, the nurse anticipates {{b2}} to "
   "{{b3}}.",
   [[("trimethoprim-sulfamethoxazole", True, "Drug of choice for Pneumocystis."),
     ("fluconazole", False, "Treats candidiasis, not Pneumocystis."),
     ("acyclovir", False, "An antiviral."),
     ("azithromycin alone", False, "Does not treat Pneumocystis.")],
    [("prednisone started with the antibiotic", True, "Steroids within 72 hours improve survival."),
     ("intubation now", False, "Not yet indicated."),
     ("starting antiretrovirals today", False, "Usually within 2 weeks, not before the infection is treated."),
     ("stopping all oxygen", False, "Dangerous.")],
    [("reduce the lung inflammation when organisms die", True, "Dying organisms worsen inflammation."),
     ("treat the oral thrush", False, "Not the reason."),
     ("raise the CD4 count", False, "Not the reason."),
     ("prevent kidney injury", False, "Not the reason.")]],
   "TMP-SMX plus steroids for moderate to severe Pneumocystis.",
   "Prioritize Pneumocystis treatment."),

sata(M(PPT, ABX, "generate_solutions", "difficult", IMMUNE, "ph"),
   "Which parts of the plan should the nurse include during high-dose trimethoprim-sulfamethoxazole?",
   [("Monitor potassium", True, "Trimethoprim raises potassium."),
    ("Watch for rash and blistering", True, "Severe skin reactions."),
    ("Check blood counts", True, "Bone marrow suppression."),
    ("Encourage fluids", True, "Prevents crystals in the urine."),
    ("Expect a sulfa allergy to be safe because the infection is serious", False, "An allergy history changes the drug choice."),
    ("Stop the steroid when the fever settles on day 2", False, "Steroids run a full 21-day taper.")],
   "Potassium, skin, blood counts and hydration are monitored.",
   "Generate the treatment monitoring plan.",
   ex=[note("Orders", "TMP-SMX IV. Prednisone 40 mg twice daily. Oxygen to SpO2 at least 92%. Airborne precautions "
       "until TB is excluded.", id="hiv-orders")]),

sata(M(MOC, CONFID, "take_action", "difficult", IMMUNE, "mgmt"),
   "The client's sister asks the nurse, \"Does my brother have AIDS? Nobody will tell me.\" The client has not "
   "given permission to share health information. Which actions are appropriate?",
   [("Tell the sister that the nurse cannot share the client's health information", True, "Confidentiality."),
    ("Ask the client privately what information may be shared and with whom", True, "Respects the client's wishes."),
    ("Offer to support the client if they choose to tell the family", True, "Therapeutic support."),
    ("Explain the diagnosis to the sister since she is a close relative", False, "HIV status needs specific consent to share."),
    ("Leave the chart open at the station so the family can read the results", False, "A privacy breach.")],
   "HIV information is shared only with the client's consent.",
   "Take action to protect confidentiality.",
   ex=[note("Day 3", "Sputum: Pneumocystis jirovecii. TB tests negative; airborne precautions stopped.", id="hiv-day3")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", IMMUNE, "ph ms"),
   "Two weeks after starting antiretroviral therapy, for each finding or statement, indicate whether it shows "
   "improvement or understanding, or needs follow-up.",
   ["Improvement or understanding", "Needs follow-up"],
   [("SpO2 96% on room air with walking", 0, "Pneumonia resolving."),
    ("\"I take my HIV medicines at the same time every day and never skip.\"", 0, "Adherence prevents resistance."),
    ("New fever and swollen neck lymph nodes after starting ART", 1, "Possible immune reconstitution inflammatory syndrome."),
    ("\"Once my viral load is undetectable I can stop the pills.\"", 1, "Lifelong therapy."),
    ("\"Undetectable means I can't pass HIV to a partner through sex.\"", 0, "Undetectable equals untransmittable."),
    ("\"I'll keep taking the TMP-SMX until my CD4 is above 200 for months.\"", 0, "Correct.")],
   "Improvement with ART; new inflammation or plans to stop therapy need follow-up.",
   "Evaluate HIV treatment and teaching.",
   ex=[note("Week 2 of ART", "Viral load falling. Discharged on ART and TMP-SMX.", id="hiv-art")]),
]

# ================================================================================================
# Case 2: Lupus flare with lupus nephritis
# ================================================================================================
sle_tabs = [
    ("sle-notes", "Clinic notes", [note("Rheumatology clinic",
        "A 26-year-old client with systemic lupus erythematosus (SLE) stopped hydroxychloroquine 2 months ago because "
        "of cost. The client has painful swollen hands, a facial rash after a beach holiday, ankle swelling and foamy "
        "urine.", id="sle-note")]),
    ("sle-labs", "Results", [table("Results", ["Today", "6 months ago"], [
        ("Blood pressure", ["156/98 mm Hg", "118/74"]),
        ("Creatinine", ["1.5 mg/dL", "0.8"]),
        ("Urine protein", ["3+, protein-creatinine ratio 2.4 g/g", "Negative"]),
        ("Urine", ["Red cell casts", "Clear"]),
        ("Complement C3 and C4", ["Low", "Normal"]),
        ("Anti-dsDNA", ["High", "Mildly raised"]),
        ("Platelets", ["98,000/mm3", "220,000"])], id="sle-table")]),
]

sle_qs = [
hl_table(M(PA, AUTO, "recognize_cues", "moderate", IMMUNE, "ms lab"),
   "Highlight the findings that suggest the kidneys are involved.",
   ["Finding", "Today"],
   [(["Urine protein", "3+, ratio 2.4 g/g"], True, "Heavy proteinuria."),
    (["Urine", "Red cell casts"], True, "Glomerular inflammation."),
    (["Creatinine", "1.5 mg/dL"], True, "Reduced kidney function."),
    (["Blood pressure", "156/98 mm Hg"], True, "New hypertension from nephritis."),
    (["Facial rash", "After sun exposure"], False, "A lupus flare sign, but not a kidney sign.")],
   "Protein, casts, rising creatinine and new hypertension mean lupus nephritis.",
   "Recognize cues of lupus nephritis."),

matrix_mc(M(PA, AUTO, "analyze_cues", "difficult", IMMUNE, "ms"),
   "For each finding, indicate whether it shows active lupus or is not explained by lupus activity.",
   ["Shows active lupus", "Not explained by lupus activity"],
   [("Low complement C3 and C4", 0, "Complement is consumed in active disease."),
    ("Rising anti-dsDNA", 0, "Tracks activity, especially nephritis."),
    ("Rash after sun exposure", 0, "Photosensitivity triggers flares."),
    ("Low platelets", 0, "Immune destruction."),
    ("Stopping hydroxychloroquine", 0, "A common trigger of flares."),
    ("Cholesterol of 182 mg/dL", 1, "Normal; not related.")],
   "Several markers show an active flare triggered by stopping medicine and sun exposure.",
   "Analyze lupus activity."),

cloze(M(PA, AUTO, "prioritize_hypotheses", "difficult", IMMUNE, "ms"),
   "Complete the sentence by choosing from the lists of options.",
   "The most serious threat is {{b1}}, which needs {{b2}}.",
   [[("lupus nephritis leading to kidney failure", True, "Major organ threat."),
     ("joint pain", False, "Painful but not organ-threatening."),
     ("the facial rash", False, "Not organ-threatening."),
     ("mild thrombocytopenia", False, "Not yet dangerous.")],
    [("a kidney biopsy and induction immunosuppression", True, "Classifies and treats nephritis."),
     ("ibuprofen for the joints only", False, "NSAIDs harm the kidneys."),
     ("watchful waiting for 3 months", False, "Kidney damage progresses."),
     ("an antibiotic", False, "Not infection.")]],
   "Lupus nephritis is organ-threatening and needs biopsy and immunosuppression.",
   "Prioritize the lupus flare."),

grouping(M(PPT, IMMUNO, "generate_solutions", "difficult", IMMUNE, "ph ms"),
   "For each area, select the actions the plan should include.",
   [("Medicines", [
       ("Restart hydroxychloroquine", True, "Reduces flares."),
       ("IV methylprednisolone then oral prednisone", True, "Rapid control."),
       ("Mycophenolate as induction", True, "Standard for nephritis."),
       ("Ibuprofen regularly for joint pain", False, "Harms the kidneys.")]),
    ("Monitoring", [
       ("Blood pressure and daily weight", True, "Nephritis and fluid."),
       ("Baseline eye exam for hydroxychloroquine", True, "Retinal toxicity screening.")]),
    ("Reproductive health", [
       ("Reliable contraception before mycophenolate", True, "Mycophenolate causes birth defects."),
       ("Advise that pregnancy is safest during a flare", False, "Pregnancy is safest after 6 months of remission.")])],
   "Immunosuppression, kidney monitoring, eye screening and contraception.",
   "Generate the lupus nephritis plan.",
   ex=[note("Biopsy", "Class IV lupus nephritis. Start induction.", id="sle-biopsy")]),

sata(M(SIC, HAI, "take_action", "difficult", IMMUNE, "ph ms"),
   "During high-dose steroids and mycophenolate, which actions should the nurse take to keep the client safe?",
   [("Teach hand hygiene and avoiding sick contacts", True, "Infection risk."),
    ("Check glucose", True, "Steroid hyperglycemia."),
    ("Report a fever even if it is low grade", True, "Steroids blunt fever."),
    ("Give a live zoster vaccine before discharge", False, "Live vaccines are avoided on immunosuppression."),
    ("Stop prednisone at once if the client feels well", False, "It must be tapered.")],
   "Infection prevention, glucose checks and no live vaccines on immunosuppression.",
   "Take action for safe immunosuppression.",
   ex=[note("Day 4", "Methylprednisolone pulses done. Prednisone 60 mg daily. Mycophenolate started.", id="sle-day4")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", IMMUNE, "ms"),
   "At 3 months, for each statement or finding, indicate whether it shows improvement or understanding, or needs "
   "follow-up.",
   ["Improvement or understanding", "Needs follow-up"],
   [("Protein-creatinine ratio 0.6 g/g", 0, "Improving."),
    ("\"I wear SPF 50 and a hat and avoid midday sun.\"", 0, "Prevents flares."),
    ("\"I'm planning to get pregnant next month.\"", 1, "Mycophenolate must be switched and disease controlled first."),
    ("\"I had my eye exam for hydroxychloroquine.\"", 0, "Correct."),
    ("Blood pressure 124/78 mm Hg", 0, "Controlled."),
    ("\"I'll use the patient assistance program so I don't stop my pills again.\"", 0, "Addresses the cost barrier.")],
   "Kidney function improved; pregnancy planning needs counseling.",
   "Evaluate lupus care.",
   ex=[note("3 months", "Creatinine 1.0 mg/dL. C3 normal.", id="sle-3m")]),
]

# ================================================================================================
# Case 3: Rheumatoid arthritis on methotrexate and a TNF inhibitor with a serious infection
# ================================================================================================
ra_tabs = [
    ("ra-notes", "Clinic and admission notes", [note("Admission",
        "A 58-year-old client with rheumatoid arthritis takes methotrexate 20 mg once weekly, folic acid and "
        "adalimumab every 2 weeks. The client has had 6 weeks of night sweats, a cough and 4 kg weight loss. The "
        "client was born in a country where TB is common. A latent TB test before adalimumab was not found in the "
        "chart.", id="ra-note")]),
    ("ra-data", "Findings", [table("Findings", ["Result"], [
        ("Temperature", ["38.2 C"]),
        ("Chest x-ray", ["Right upper lobe cavity"]),
        ("Sputum smear", ["Acid-fast bacilli seen"]),
        ("WBC", ["3.1 x 10^3/mm3"]),
        ("ALT", ["96 units/L"]),
        ("Mouth", ["Painful ulcers"])], id="ra-table")]),
]

ra_qs = [
hl_table(M(PA, ARTH, "recognize_cues", "moderate", IMMUNE, "ms"),
   "Highlight the findings that require follow-up.",
   ["Finding", "Result"],
   [(["Chest x-ray and smear", "Cavity, acid-fast bacilli"], True, "Active TB."),
    (["WBC", "3.1"], True, "Methotrexate marrow suppression."),
    (["ALT", "96 units/L"], True, "Methotrexate liver toxicity."),
    (["Mouth", "Painful ulcers"], True, "Methotrexate toxicity."),
    (["Night sweats and weight loss", "6 weeks"], True, "TB symptoms.")],
   "Active TB plus signs of methotrexate toxicity.",
   "Recognize cues of complications of RA therapy."),

matrix_mr(M(PPT, IMMUNO, "analyze_cues", "difficult", IMMUNE, "ph"),
   "For each finding, select the medicines that likely contributed. Each row may have more than one answer.",
   ["Methotrexate", "Adalimumab"],
   [("Reactivated TB", {1}, "TNF inhibitors allow latent TB to reactivate."),
    ("Low WBC", {0}, "Marrow suppression."),
    ("Raised ALT", {0}, "Liver toxicity."),
    ("Mouth ulcers", {0}, "Mucositis."),
    ("Higher risk of serious infections", {0, 1}, "Both suppress immunity.")],
   "TNF inhibitors reactivate TB; methotrexate harms marrow, liver and mouth.",
   "Analyze adverse effects of RA medicines."),

cloze(M(PPT, IMMUNO, "prioritize_hypotheses", "difficult", IMMUNE, "ph ms"),
   "Complete the sentences by choosing from the lists of options.",
   "The priority is {{b1}}. The likely system failure was {{b2}}.",
   [[("airborne isolation, holding adalimumab and starting TB treatment", True, "Protects others and treats TB."),
     ("increasing methotrexate to control joint pain", False, "Worsens toxicity."),
     ("discharge home with a mask", False, "Smear-positive TB needs isolation."),
     ("a steroid burst", False, "Worsens TB.")],
    [("missing TB screening before the TNF inhibitor", True, "Screening is required before starting."),
     ("taking folic acid", False, "Folic acid reduces toxicity."),
     ("weekly methotrexate dosing", False, "Correct dosing."),
     ("a vaccine given last year", False, "Not related.")]],
   "Isolate, hold biologics and treat TB; screening was missed.",
   "Prioritize TB on a TNF inhibitor."),

matrix_mc(M(SIC, TBP, "generate_solutions", "difficult", IMMUNE, "mgmt"),
   "For each measure, indicate whether it is needed or not needed for this client.",
   ["Needed", "Not needed"],
   [("Negative-pressure room", 0, "Airborne precautions."),
    ("Fitted N95 respirator for staff", 0, "Airborne."),
    ("Surgical mask on the client when leaving the room", 0, "Source control."),
    ("Report the case to public health", 0, "TB is reportable."),
    ("Droplet precautions only with a surgical mask for staff", 1, "TB needs airborne precautions."),
    ("Keep the door open for observation", 1, "Breaks negative pressure.")],
   "Airborne precautions and public health reporting.",
   "Generate the isolation plan.",
   ex=[note("Orders", "Hold methotrexate and adalimumab. Rifampin, isoniazid, pyrazinamide, ethambutol. Pyridoxine.", id="ra-orders")]),

sata(M(MOC, "quality-improvement-and-incident-reporting", "take_action", "difficult", IMMUNE, "mgmt"),
   "The nurse learns that the TB test before adalimumab was never done. Which actions are appropriate?",
   [("Complete an incident report", True, "Captures the system failure."),
    ("Tell the provider so the client can be informed", True, "Disclosure."),
    ("Suggest a checklist that blocks biologic orders without a TB result", True, "Prevents recurrence."),
    ("Write in the chart that the clinic made a mistake", False, "Charts record facts, not blame."),
    ("Keep the information private to avoid upsetting the client", False, "Clients have a right to know.")],
   "Report, disclose and fix the system.",
   "Take action on a screening error.",
   ex=[note("Day 2", "Clinic records confirm no TB screening before adalimumab.", id="ra-day2")]),

matrix_mc(M(PPT, ABX, "evaluate_outcomes", "difficult", IMMUNE, "ph"),
   "At 2 weeks of TB treatment, for each finding or statement, indicate whether it shows the expected course or "
   "needs follow-up.",
   ["Expected", "Needs follow-up"],
   [("Orange-red urine and tears", 0, "Rifampin effect."),
    ("Afebrile, cough improving", 0, "Responding."),
    ("New blurred vision and trouble seeing red and green", 1, "Ethambutol optic neuritis."),
    ("Yellow skin and dark urine", 1, "Hepatotoxicity."),
    ("Numb, tingling feet despite pyridoxine", 1, "Isoniazid neuropathy."),
    ("\"I'll take the TB pills for at least 6 months.\"", 0, "Correct.")],
   "Expected rifampin effects; vision, liver and nerve symptoms need follow-up.",
   "Evaluate TB treatment.",
   ex=[note("2 weeks", "Smears falling. RA plan on hold.", id="ra-2wk")]),
]

# ================================================================================================
# Case 4: Candidemia and antifungal therapy
# ================================================================================================
cand_tabs = [
    ("cand-notes", "ICU notes", [note("Surgical ICU",
        "A 64-year-old client is on day 12 after a bowel resection with total parenteral nutrition through a "
        "central line and broad-spectrum antibiotics. New fever and hypotension developed overnight despite the "
        "antibiotics.", id="cand-note")]),
    ("cand-data", "Results", [table("Results", ["Result"], [
        ("Temperature", ["38.9 C"]),
        ("Blood pressure", ["92/54 mm Hg"]),
        ("Blood cultures (central and peripheral)", ["Yeast seen; Candida glabrata"]),
        ("Eye exam", ["Pending"]),
        ("Creatinine", ["1.0 mg/dL"]),
        ("Potassium", ["3.9 mEq/L"])], id="cand-table")]),
]

cand_qs = [
hl_table(M(PA, SEPSIS, "recognize_cues", "moderate", IMMUNE, "ms"),
   "Highlight the risk factors for fungal bloodstream infection.",
   ["Risk factor", "Present"],
   [(["Central line", "Yes"], True, "Line colonization."),
    (["Total parenteral nutrition", "Yes"], True, "Glucose-rich fluid supports Candida."),
    (["Broad-spectrum antibiotics", "Yes"], True, "Kill normal flora."),
    (["Recent abdominal surgery", "Yes"], True, "Gut barrier breach."),
    (["Potassium", "3.9 mEq/L"], False, "Not a risk factor.")],
   "Lines, TPN, antibiotics and surgery raise Candida risk.",
   "Recognize risk factors for candidemia."),

matrix_mc(M(PPT, ABX, "analyze_cues", "difficult", IMMUNE, "ph"),
   "For each antifungal, indicate its main use.",
   ["First-line for candidemia", "Oral or vaginal yeast, or step-down", "Severe or resistant fungal infection"],
   [("Micafungin (an echinocandin)", 0, "First-line for candidemia, especially C. glabrata."),
    ("Fluconazole", 1, "Thrush, vaginal yeast and step-down when sensitive."),
    ("Amphotericin B", 2, "Severe or resistant infections."),
    ("Nystatin suspension", 1, "Oral thrush.")],
   "Echinocandins first for candidemia; amphotericin for severe or resistant cases.",
   "Analyze antifungal choices."),

cloze(M(PA, SEPSIS, "prioritize_hypotheses", "difficult", IMMUNE, "ms"),
   "Complete the sentence by choosing from the lists of options.",
   "Besides an antifungal, the most important step is {{b1}}, and every client with candidemia needs {{b2}}.",
   [[("removing the central line", True, "The line is the likely source."),
     ("changing the TPN bag", False, "Not enough."),
     ("adding a third antibiotic", False, "Not bacterial."),
     ("waiting for the fever to settle", False, "Delays source control.")],
    [("a dilated eye exam for fungal endophthalmitis", True, "Changes length of treatment."),
     ("a colonoscopy", False, "Not needed."),
     ("a lumbar puncture", False, "Not routine."),
     ("a bone scan", False, "Not routine.")]],
   "Remove the line and check the eyes.",
   "Prioritize candidemia management."),

sata(M(PPT, ABX, "generate_solutions", "difficult", IMMUNE, "ph"),
   "On day 5, cultures still grow Candida and the provider changes to liposomal amphotericin B. Which actions "
   "should the nurse anticipate?",
   [("Premedicate as prescribed", True, "Reduces infusion reactions."),
    ("Give a saline bolus before the dose", True, "Protects the kidneys."),
    ("Check creatinine, potassium and magnesium", True, "Nephrotoxic and wastes electrolytes."),
    ("Mix the drug in 0.9% sodium chloride", False, "Amphotericin is mixed in dextrose 5%; saline makes it precipitate."),
    ("Give it as a rapid IV push over 2 minutes to limit vein irritation", False, "Infused over 2 hours or more.")],
   "Premedication, saline loading and kidney and electrolyte monitoring.",
   "Generate the amphotericin plan.",
   ex=[note("Day 5", "Line removed day 1. Cultures still positive. Eye exam: no endophthalmitis.", id="cand-day5")]),

sata(M(PPT, ABX, "take_action", "difficult", IMMUNE, "ph"),
   "Thirty minutes into the amphotericin infusion, review the findings. Which actions should the nurse take?",
   [("Slow or pause the infusion and notify the provider", True, "Infusion reaction."),
    ("Give meperidine for rigors as prescribed", True, "Treats rigors."),
    ("Monitor vital signs every 15 minutes", True, "Tracks the reaction."),
    ("Increase the rate to finish before the rigors worsen", False, "Worsens the reaction."),
    ("Mark the client as allergic and never give the drug again", False, "Infusion reactions are not allergy; premedication is adjusted.")],
   "Rigors and fever during amphotericin are an infusion reaction.",
   "Take action for an amphotericin infusion reaction.",
   ex=[flow("Infusion", ["Start", "30 min"], [
       ("Temperature", ["37.4", "38.8 C"]),
       ("Client", ["Comfortable", "Shaking chills"]),
       ("Blood pressure", ["118/70", "112/68 mm Hg"])], id="cand-30min")]),

matrix_mc(M(RRP, LABS, "evaluate_outcomes", "difficult", IMMUNE, "ph lab"),
   "On day 10, for each finding, indicate whether it shows improvement or needs follow-up.",
   ["Improved", "Needs follow-up"],
   [("Blood cultures negative for 3 days", 0, "Clearing."),
    ("Afebrile", 0, "Responding."),
    ("Creatinine 2.0 mg/dL (1.0 at start)", 1, "Nephrotoxicity."),
    ("Potassium 2.9 mEq/L", 1, "Renal wasting."),
    ("Magnesium 1.3 mg/dL", 1, "Renal wasting."),
    ("Blood pressure 124/76 mm Hg", 0, "Shock resolved.")],
   "Infection cleared; kidney and electrolyte effects need treatment.",
   "Evaluate antifungal therapy.",
   ex=[note("Day 10", "Plan: 14 days after the first negative culture.", id="cand-day10")]),
]

cases = [
    case("Advanced HIV with Pneumocystis pneumonia", hiv_tabs, hiv_qs),
    case("Lupus flare with lupus nephritis", sle_tabs, sle_qs),
    case("Rheumatoid arthritis biologic therapy with reactivated tuberculosis", ra_tabs, ra_qs),
    case("Candidemia treated with antifungal drugs", cand_tabs, cand_qs),
]

BUILT = build(124, "immunity-cases-hiv-sle-ra-fungal",
              "Immunity and infection case studies: HIV with Pneumocystis, lupus nephritis, RA biologics with TB and "
              "candidemia",
              items, cases, case_conversion=True)
