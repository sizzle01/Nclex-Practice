"""Every topic and subtopic of the user's nursing review book, mapped to the case studies that cover it.

The user's rule (2026-11-21): NCLEX-standard questions of all item types for every subtopic, with case studies
first and difficult items prioritized, because a repeat candidate practices them to pass the real exam. coverage.py
writes this checklist into PROGRESS.md, and build_all.py stops if a listed case title does not exist, so the map
cannot drift from the bank. Add each new case to the subtopics it teaches when its chunk is written.
"""

# (section, subtopic, [case study titles that cover it in depth])
BOOK = [
    # ---- Fundamental ----
    ("Fundamental", "ABG", ["Status asthmaticus in an adult", "COPD with CO2 narcosis from too much oxygen",
                            "ARDS on mechanical ventilation", "Euglycemic DKA on an SGLT2 inhibitor after surgery"]),
    ("Fundamental", "ABO blood group", ["Warfarin over-anticoagulation with a GI bleed and a mechanical valve",
                                       "Lower GI bleed on apixaban"]),
    ("Fundamental", "Basic EKG concept", ["Drug-induced complete heart block and transcutaneous pacing",
                                          "Ventricular tachycardia in a client with an ICD",
                                          "Permanent pacemaker failure to capture",
                                          "New atrial fibrillation with a rapid rate from thyrotoxicosis"]),
    ("Fundamental", "Dosage calculation", ["Heparin-induced thrombocytopenia with arterial thrombosis",
                                           "Cardiogenic shock with a pulmonary artery catheter",
                                           "Chemotherapy-induced nausea and vomiting", "Acetaminophen overdose",
                                           "Diabetic foot ulcer with osteomyelitis"]),
    ("Fundamental", "IV fluid", ["Hyperosmolar hyperglycemic state", "SIADH with a hyponatremic seizure"]),
    ("Fundamental", "Electrolyte imbalances", ["Heart failure with digoxin toxicity", "SIADH with a hyponatremic seizure",
                                               "Adrenal crisis in Addison disease",
                                               "Refeeding syndrome on parenteral nutrition",
                                               "Hematoma and hypocalcemia after thyroidectomy"]),
    ("Fundamental", "Lab", ["Severe community-acquired pneumonia in an older adult", "Acute cholangitis and ERCP",
                            "Acute hepatitis B and exposures", "Small bowel obstruction with strangulation"]),
    # ---- Holistic health assessment ----
    ("Holistic health assessment", "Head-to-toe assessment", []),
    ("Holistic health assessment", "Head-to-toe assessment template", []),
    ("Holistic health assessment", "Cranial nerve", []),
    # ---- Pharmacology ----
    ("Pharmacology", "Antimicrobials: antibiotics", ["Active pulmonary tuberculosis", "Cystic fibrosis exacerbation",
                                                    "Infective endocarditis in a client who injects drugs",
                                                    "Severe C. difficile colitis and toxic megacolon"]),
    ("Pharmacology", "Antimicrobials: antiviral", ["Influenza outbreak in long-term care"]),
    ("Pharmacology", "Antimicrobials: antifungal", []),
    ("Pharmacology", "Antimicrobials: antiparasitic", []),
    ("Pharmacology", "Antilipidemics", []),
    ("Pharmacology", "Hematopoietic medications", []),
    ("Pharmacology", "Cardiovascular: ANS overview", ["Pheochromocytoma",
                                                     "Anaphylactic shock in a client taking a beta blocker"]),
    ("Pharmacology", "Cardiovascular: adrenergic agonists", ["Anaphylactic shock in a client taking a beta blocker",
                                                            "Cardiogenic shock with a pulmonary artery catheter"]),
    ("Pharmacology", "Cardiovascular: adrenergic antagonists", ["Pheochromocytoma", "Type A aortic dissection"]),
    ("Pharmacology", "Cardiovascular: PSNS overview and cholinergic medications", []),
    ("Pharmacology", "Cardiovascular: anticholinergic", ["Drug-induced complete heart block and transcutaneous pacing",
                                                        "Over-the-counter cold medicines in an older adult"]),
    ("Pharmacology", "Cardiovascular: hypertension", ["Hypertensive emergency after stopping clonidine"]),
    ("Pharmacology", "Cardiovascular: heart failure", ["Heart failure with digoxin toxicity",
                                                      "Acute pulmonary edema in heart failure"]),
    ("Pharmacology", "Cardiovascular: antidysrhythmic", ["Ventricular tachycardia in a client with an ICD",
                                                        "New atrial fibrillation with a rapid rate from thyrotoxicosis"]),
    ("Pharmacology", "Cardiovascular: angina", ["NSTEMI with a retroperitoneal bleed after catheterization"]),
    ("Pharmacology", "Cardiovascular: fluid volume excess", ["Acute pulmonary edema in heart failure",
                                                            "Ascites, paracentesis and spontaneous bacterial peritonitis"]),
    ("Pharmacology", "Coagulation: anticoagulants", ["Heparin-induced thrombocytopenia with arterial thrombosis",
                                                    "Warfarin over-anticoagulation with a GI bleed and a mechanical valve",
                                                    "Lower GI bleed on apixaban"]),
    ("Pharmacology", "Coagulation: antiplatelets", ["NSTEMI with a retroperitoneal bleed after catheterization"]),
    ("Pharmacology", "Coagulation: thrombolytics", ["Acute ischemic stroke in the thrombolytic window"]),
    ("Pharmacology", "Pain, fever and inflammation", ["Acetaminophen overdose",
                                                     "Opioid-induced respiratory depression in sleep apnea"]),
    ("Pharmacology", "Endocrine: thyroid and antithyroid", ["Thyroid storm", "Myxedema coma",
                                                           "Levothyroxine over-replacement in an older adult"]),
    ("Pharmacology", "Endocrine: osteoporosis", ["Osteoporosis with a vertebral compression fracture"]),
    ("Pharmacology", "Endocrine: antidiabetic", ["Newly diagnosed type 2 diabetes",
                                                "Prolonged hypoglycemia from a sulfonylurea"]),
    ("Pharmacology", "Endocrine: insulin", ["Hyperosmolar hyperglycemic state", "Insulin pump failure leading to DKA",
                                           "Euglycemic DKA on an SGLT2 inhibitor after surgery"]),
    ("Pharmacology", "GI: PUD and GERD", ["Perforated duodenal ulcer"]),
    ("Pharmacology", "GI: antiemetics", ["Chemotherapy-induced nausea and vomiting"]),
    ("Pharmacology", "Neurologic", ["Status epilepticus after missed antiseizure doses"]),
    ("Pharmacology", "Respiratory: asthma and bronchoconstrictive disorders", ["Status asthmaticus in an adult",
                                                                              "COPD with CO2 narcosis from too much oxygen"]),
    ("Pharmacology", "Respiratory: allergic disorders", ["Over-the-counter cold medicines in an older adult"]),
    ("Pharmacology", "Respiratory: decongestant, antitussive, expectorant, mucolytic",
     ["Over-the-counter cold medicines in an older adult"]),
    # ---- Mental health nursing ----
    ("Mental health", "Anxiety disorders and OCD", []),
    ("Mental health", "Bipolar mood disorders", []),
    ("Mental health", "Childhood and neurodevelopmental disorders", []),
    ("Mental health", "Defense mechanisms", []),
    ("Mental health", "Eating disorders", []),
    ("Mental health", "Impulse control disorders", []),
    ("Mental health", "Major depressive disorders", []),
    ("Mental health", "Personality disorders", []),
    ("Mental health", "Schizophrenia spectrum disorders", []),
    ("Mental health", "Somatic symptom disorders", []),
    ("Mental health", "Substance use disorders", ["Alcohol withdrawal after orthopedic surgery"]),
    ("Mental health", "Trauma, stressor-related and dissociative disorders", []),
    ("Mental health", "Medications: anxiolytics", []),
    ("Mental health", "Medications: depression and mood stabilization", []),
    ("Mental health", "Medications: psychotic disorders", []),
    # ---- Maternity ----
    ("Maternity", "Abruptio placentae", []),
    ("Maternity", "Electronic fetal heart rate monitoring", []),
    ("Maternity", "Fetal growth and development", []),
    ("Maternity", "Signs of pregnancy", []),
    ("Maternity", "Measuring fundal height", []),
    ("Maternity", "Gestational diabetes mellitus", []),
    ("Maternity", "Labor and delivery", []),
    ("Maternity", "Medications used during labor and delivery", []),
    ("Maternity", "Newborn", []),
    ("Maternity", "Newborn complications", []),
    ("Maternity", "Placenta previa", []),
    ("Maternity", "Postpartum", ["Postpartum hemorrhage from uterine atony"]),
    ("Maternity", "Preeclampsia", ["Preeclampsia with severe features at 35 weeks"]),
    ("Maternity", "Pregnancy terminology", []),
    ("Maternity", "Rh incompatibility", []),
    # ---- Pediatrics ----
    ("Pediatrics", "Growth and development: infant", []),
    ("Pediatrics", "Growth and development: toddler", []),
    ("Pediatrics", "Growth and development: preschool", []),
    ("Pediatrics", "Growth and development: school age", []),
    ("Pediatrics", "Growth and development: adolescence", []),
    ("Pediatrics", "Cardiac disorders", []),
    ("Pediatrics", "Genitourinary disorders", []),
    ("Pediatrics", "Hematologic disorders", ["Sickle cell crisis with acute chest syndrome"]),
    ("Pediatrics", "Neurologic disorders", []),
    ("Pediatrics", "Respiratory disorders", ["Infant with RSV bronchiolitis", "Child with a severe asthma exacerbation"]),
    ("Pediatrics", "Vaccination schedule", []),
    # ---- Med-Surg 1 & 2 ----
    ("Med-Surg", "Altered immunity disorders", ["Anaphylactic shock in a client taking a beta blocker",
                                                "Crohn disease flare and infliximab"]),
    ("Med-Surg", "Cardiac disorders", ["Inferior ST-elevation myocardial infarction", "Type A aortic dissection",
                                       "Cardiac tamponade after coronary bypass surgery"]),
    ("Med-Surg", "Endocrine disorders", ["Adrenal crisis in Addison disease", "Diabetes insipidus after pituitary surgery"]),
    ("Med-Surg", "Gastrointestinal disorders", ["Hepatic encephalopathy in cirrhosis", "Severe acute pancreatitis"]),
    ("Med-Surg", "Hemodynamic monitoring", ["Cardiogenic shock with a pulmonary artery catheter",
                                            "Arterial line and CVP monitoring after open AAA repair"]),
    ("Med-Surg", "Musculoskeletal disorders", ["Hemorrhage after a total hip replacement",
                                               "Acute limb ischemia and compartment syndrome"]),
    ("Med-Surg", "Neurologic disorders", ["Rising intracranial pressure after a head injury", "Acute ischemic stroke"]),
    ("Med-Surg", "Pacemaker", ["Permanent pacemaker failure to capture",
                               "Drug-induced complete heart block and transcutaneous pacing"]),
    ("Med-Surg", "Pulmonary disorders", ["ARDS on mechanical ventilation", "Active pulmonary tuberculosis"]),
    ("Med-Surg", "Renal disorders", ["Acute kidney injury with hyperkalemia"]),
    ("Med-Surg", "Sepsis", ["Older adult with sepsis from a urinary tract infection",
                            "Septic shock from an anastomotic leak after colectomy"]),
    ("Med-Surg", "Sexually transmitted infections", []),
    ("Med-Surg", "Shock", ["Cardiogenic shock with a pulmonary artery catheter",
                           "Anaphylactic shock in a client taking a beta blocker"]),
    ("Med-Surg", "Trauma", ["Flail chest with pulmonary contusion", "Tension pneumothorax after a car crash",
                            "Major burn with inhalation injury"]),
    ("Med-Surg", "Transplant rejection", []),
]

# A subtopic counts as covered with at least this many cases; one case alone is "started".
COVERED_CASES = 2


def check(case_titles):
    """Raises if the map names a case that does not exist."""
    missing = sorted({t for _, _, titles in BOOK for t in titles} - set(case_titles))
    if missing:
        raise SystemExit("book_topics.py names cases that do not exist:\n  " + "\n  ".join(missing))


def markdown():
    """The checklist section for PROGRESS.md."""
    covered = sum(1 for _, _, t in BOOK if len(t) >= COVERED_CASES)
    started = sum(1 for _, _, t in BOOK if 0 < len(t) < COVERED_CASES)
    out = ["", "## Review book checklist (book_topics.py)", "",
           f"**{covered} of {len(BOOK)} subtopics covered** (at least {COVERED_CASES} case studies), {started} started "
           f"(1 case), {len(BOOK) - covered - started} not yet covered.", "",
           "| Section | Subtopic | Status | Case studies |", "|---|---|---|---|"]
    for section, subtopic, titles in BOOK:
        status = "covered" if len(titles) >= COVERED_CASES else "started" if titles else "**not yet**"
        out.append(f"| {section} | {subtopic} | {status} | {'; '.join(titles) or '-'} |")
    return out


# ---- Topic tests (F27) ------------------------------------------------------------------------------------------
# Which standalone questions belong to each subtopic's topic test, besides its case studies: a question joins when
# one of its tags is listed, or when its stem matches the keywords. Tags alone are used only when they are specific
# to the subtopic; broad tags (for example psychiatric-medications) are narrowed by keywords: (tags, keywords, both).
# "both" True means a question needs a listed tag AND a keyword match.
MATCH = {
    "ABG": ([], r"\babg\b|arterial blood gas|paco2|ph of 7\.|acidosis|alkalosis", False),
    "ABO blood group": (["blood-transfusion-reactions"], r"blood type|abo\b|crossmatch|transfus|packed red|rh-negative", False),
    "Basic EKG concept": ([], r"\becg\b|\bekg\b|rhythm strip|telemetry|p wave|qrs|st elevation|st depression|dysrhythm|atrial fib|ventricular tach|heart block", False),
    "Dosage calculation": (["medication-dose-calculations"], None, False),
    "IV fluid": (["iv-fluid-selection", "iv-therapy-and-vascular-access", "fluid-volume-imbalances"], r"isotonic|hypotonic|hypertonic|lactated ringer|0\.9% sodium chloride|d5w|infiltrat", False),
    "Electrolyte imbalances": (["electrolyte-imbalances", "potassium-medication-safety", "electrolyte-and-glucose-lab-results"], r"hyperkal|hypokal|hyponatr|hypernatr|hypocalc|hypercalc|hypomagnes|potassium|sodium level|magnesium level", False),
    "Lab": (["laboratory-values-and-monitoring", "specimen-collection"], r"laboratory result|lab result", False),
    "Head-to-toe assessment": ([], r"head-to-toe|physical assessment|assessment technique", False),
    "Head-to-toe assessment template": ([], r"admission assessment|focused assessment|shift assessment|assessment findings", False),
    "Cranial nerve": ([], r"cranial nerve|pupil|gag reflex|facial droop|extraocular|trigeminal|vagus|oculomotor", False),
    "Antimicrobials: antibiotics": (["antimicrobial-therapy"], r"antibiotic|vancomycin|gentamicin|tobramycin|penicillin|cephalosporin|ceftriaxone|ciprofloxacin|clindamycin|metronidazole|isoniazid|rifampin", False),
    "Antimicrobials: antiviral": ([], r"antiviral|acyclovir|valacyclovir|oseltamivir|antiretroviral|zidovudine|tenofovir", False),
    "Antimicrobials: antifungal": ([], r"antifungal|fluconazole|nystatin|amphotericin|clotrimazole|candida|thrush", False),
    "Antimicrobials: antiparasitic": ([], r"antiparasit|metronidazole|lice|pinworm|scabies|permethrin|malaria|ivermectin", False),
    "Antilipidemics": ([], r"statin|atorvastatin|simvastatin|rosuvastatin|cholesterol|ldl|lipid|niacin|ezetimibe", False),
    "Hematopoietic medications": (["hematologic-medications"], r"epoetin|erythropoietin|filgrastim|iron|ferrous|vitamin b12|folic acid|darbepoetin", False),
    "Cardiovascular: ANS overview": ([], r"sympathetic|parasympathetic|autonomic|fight or flight", False),
    "Cardiovascular: adrenergic agonists": ([], r"epinephrine|norepinephrine|dopamine|dobutamine|phenylephrine|albuterol|vasopressor", False),
    "Cardiovascular: adrenergic antagonists": ([], r"metoprolol|propranolol|carvedilol|atenolol|labetalol|esmolol|beta.?blocker|prazosin|doxazosin|tamsulosin|alpha.?blocker", False),
    "Cardiovascular: PSNS overview and cholinergic medications": ([], r"cholinergic|bethanechol|neostigmine|pyridostigmine|myasthenia|edrophonium|donepezil", False),
    "Cardiovascular: anticholinergic": ([], r"anticholinergic|atropine|oxybutynin|ipratropium|scopolamine|glycopyrrolate|diphenhydramine|benztropine", False),
    "Cardiovascular: hypertension": (["hypertension-management", "hypertensive-crisis"], r"hypertens|lisinopril|losartan|amlodipine|ace inhibitor|clonidine|hydralazine", False),
    "Cardiovascular: heart failure": (["heart-failure-interventions", "heart-failure-assessment", "heart-failure-treatment-response", "heart-failure-self-care-teaching", "acute-pulmonary-edema"], r"heart failure|digoxin|sacubitril", False),
    "Cardiovascular: antidysrhythmic": ([], r"amiodarone|adenosine|diltiazem|lidocaine|antidysrhythm|antiarrhythm|cardioversion", False),
    "Cardiovascular: angina": ([], r"angina|nitroglycerin|isosorbide|ranolazine", False),
    "Cardiovascular: fluid volume excess": (["fluid-volume-imbalances"], r"furosemide|bumetanide|spironolactone|hydrochlorothiazide|diuretic|fluid volume excess|fluid overload", False),
    "Coagulation: anticoagulants": (["anticoagulant-therapy-and-monitoring"], r"heparin|enoxaparin|warfarin|apixaban|rivaroxaban|dabigatran|anticoagul", False),
    "Coagulation: antiplatelets": ([], r"aspirin|clopidogrel|ticagrelor|antiplatelet|prasugrel", False),
    "Coagulation: thrombolytics": ([], r"alteplase|thromboly|tpa\b|tenecteplase", False),
    "Pain, fever and inflammation": (["opioid-safety", "pain-assessment-and-comfort"], r"opioid|morphine|hydromorphone|oxycodone|naloxone|acetaminophen|ibuprofen|nsaid|ketorolac|antipyretic", False),
    "Endocrine: thyroid and antithyroid": (["thyroid-and-parathyroid-disorders", "endocrine-medications"], r"thyroid|levothyroxine|methimazole|propylthiouracil|radioactive iodine", False),
    "Endocrine: osteoporosis": (["bone-health-and-osteoporosis"], r"osteopor|alendronate|bisphosphonate|calcitonin|raloxifene|bone density", False),
    "Endocrine: antidiabetic": (["non-insulin-diabetes-medications", "diabetes-self-management"], r"metformin|glipizide|glyburide|sulfonylurea|sglt2|empagliflozin|glp-1|semaglutide|sitagliptin", False),
    "Endocrine: insulin": (["insulin-therapy", "hypoglycemia-treatment", "recognizing-hypoglycemia", "diabetic-ketoacidosis", "hyperosmolar-hyperglycemic-state"], r"insulin", False),
    "GI: PUD and GERD": (["upper-gi-disorders-and-bleeding"], r"peptic ulcer|\bgerd\b|reflux|omeprazole|pantoprazole|famotidine|sucralfate|h\. pylori|antacid", False),
    "GI: antiemetics": ([], r"antiemetic|ondansetron|metoclopramide|prochlorperazine|promethazine|nausea and vomiting", False),
    "Neurologic": (["antiseizure-medications", "neurological-medications"], r"phenytoin|levetiracetam|carbamazepine|valproic|levodopa|carbidopa|baclofen|donepezil", False),
    "Respiratory: asthma and bronchoconstrictive disorders": (["asthma-and-copd-management", "respiratory-medications"], r"asthma|copd|albuterol|inhaler|montelukast|theophylline|tiotropium|fluticasone", False),
    "Respiratory: allergic disorders": (["latex-and-drug-allergies"], r"antihistamine|allerg|anaphyla|loratadine|cetirizine|diphenhydramine", False),
    "Respiratory: decongestant, antitussive, expectorant, mucolytic": ([], r"decongestant|pseudoephedrine|antitussive|dextromethorphan|guaifenesin|expectorant|acetylcysteine|benzonatate|codeine cough", False),
    "Anxiety disorders and OCD": (["anxiety-and-related-disorders"], r"anxiety|panic|obsessive|compulsive|phobia", False),
    "Bipolar mood disorders": ([], r"bipolar|mania|manic|lithium", False),
    "Childhood and neurodevelopmental disorders": (["neurodevelopmental-disorders"], r"adhd|autism|attention-deficit|conduct disorder|oppositional", False),
    "Defense mechanisms": (["defense-mechanisms"], r"defense mechanism", False),
    "Eating disorders": (["eating-disorders"], r"anorexia nervosa|bulimia|binge eating|eating disorder", False),
    "Impulse control disorders": ([], r"impulse control|kleptomania|pyromania|intermittent explosive|gambling", False),
    "Major depressive disorders": (["mood-disorders", "suicide-risk-and-safety"], r"depress|suicid|electroconvulsive", False),
    "Personality disorders": (["personality-disorders"], r"personality disorder|borderline|antisocial|narcissistic|manipulat", False),
    "Schizophrenia spectrum disorders": (["psychotic-disorders"], r"schizophren|hallucinat|delusion|psychos", False),
    "Somatic symptom disorders": ([], r"somatic symptom|conversion disorder|illness anxiety|hypochondri|factitious", False),
    "Substance use disorders": (["substance-use-disorders", "alcohol-withdrawal"], r"alcohol|withdrawal|opioid use|heroin|cocaine|methamphetamine|naltrexone|buprenorphine|disulfiram", False),
    "Trauma, stressor-related and dissociative disorders": (["trauma-and-stress-related-disorders", "intimate-partner-and-sexual-violence", "crisis-intervention"], r"ptsd|post-traumatic|dissociat|trauma-related|adjustment disorder", False),
    "Medications: anxiolytics": ([], r"lorazepam|alprazolam|diazepam|buspirone|benzodiazepine|anxiolytic|clonazepam", False),
    "Medications: depression and mood stabilization": ([], r"sertraline|fluoxetine|citalopram|escitalopram|paroxetine|ssri|snri|venlafaxine|bupropion|tricyclic|amitriptyline|maoi|phenelzine|lithium|valproate|lamotrigine|serotonin syndrome", False),
    "Medications: psychotic disorders": ([], r"haloperidol|olanzapine|risperidone|quetiapine|clozapine|antipsychotic|aripiprazole|neuroleptic malignant|tardive|extrapyramidal", False),
    "Abruptio placentae": ([], r"abruptio|placental abruption|abruption", False),
    "Electronic fetal heart rate monitoring": (["fetal-heart-rate-monitoring"], r"fetal heart|deceleration|variability|fhr", False),
    "Fetal growth and development": (["prenatal-testing"], r"fetal development|fetal growth|gestational age|ultrasound|amniocentesis|nonstress test", False),
    "Signs of pregnancy": ([], r"presumptive|probable sign|positive sign|chadwick|goodell|hegar|hcg|quickening", False),
    "Measuring fundal height": ([], r"fundal height|fundus|mcdonald", False),
    "Gestational diabetes mellitus": ([], r"gestational diabetes|glucose tolerance test", False),
    "Labor and delivery": (["labor-and-birth", "intrapartum-complications"], r"\blabor\b|contraction|dilat|effacement|stage of labor|epidural", False),
    "Medications used during labor and delivery": (["obstetric-medications"], r"oxytocin|magnesium sulfate|terbutaline|methylergonovine|carboprost|misoprostol|betamethasone|dinoprostone", False),
    "Newborn": (["newborn-care", "breastfeeding"], r"newborn|neonat|apgar|vitamin k injection|erythromycin ointment", False),
    "Newborn complications": (["newborn-complications"], r"hyperbilirubin|jaundice|neonatal abstinence|newborn hypoglyc|respiratory distress syndrome", False),
    "Placenta previa": ([], r"placenta previa|previa", False),
    "Postpartum": (["postpartum-care", "postpartum-hemorrhage", "perinatal-mood-disorders"], r"postpartum|lochia|uterine atony|fundal massage", False),
    "Preeclampsia": (["pregnancy-complications"], r"preeclampsia|eclampsia|hellp", False),
    "Pregnancy terminology": (["preconception-and-prenatal-care"], r"gravida|para\b|gtpal|naegele|trimester", False),
    "Rh incompatibility": ([], r"rh-negative|rho\(d\)|rhogam|rh immune|rh incompat", False),
    "Growth and development: infant": (["infant-growth-and-development", "infant-and-child-nutrition"], r"infant|month-old", False),
    "Growth and development: toddler": ([], r"toddler|temper tantrum|parallel play|\b(1|2|3)-year-old|18-month-old", False),
    "Growth and development: preschool": ([], r"preschool|4-year-old|5-year-old|magical thinking", False),
    "Growth and development: school age": ([], r"school-age|school age|7-year-old|8-year-old|9-year-old|10-year-old", False),
    "Growth and development: adolescence": (["adolescent-health"], r"adolescen|teen|15-year-old|16-year-old|17-year-old", False),
    "Pediatrics|Cardiac disorders": (["pediatric-cardiac-disorders"], r"tetralogy|ventricular septal|patent ductus|kawasaki|coarctation|congenital heart", False),
    "Pediatrics|Genitourinary disorders": (["pediatric-gi-gu-disorders"], r"nephrotic|glomerulonephritis|wilms|hypospadias|vesicoureteral|enuresis", False),
    "Pediatrics|Hematologic disorders": (["pediatric-hematology-oncology", "sickle-cell-disease"], r"sickle cell|hemophilia|leukemia|iron deficiency anemia in a child", False),
    "Pediatrics|Neurologic disorders": (["pediatric-neurological-disorders"], r"febrile seizure|hydroceph|spina bifida|cerebral palsy|reye|meningitis in a child", False),
    "Pediatrics|Respiratory disorders": (["pediatric-respiratory-disorders"], r"croup|epiglottitis|bronchiolitis|rsv|cystic fibrosis|child.*asthma", False),
    "Vaccination schedule": (["immunizations", "childhood-communicable-diseases"], r"vaccin|immuniz|mmr|dtap|varicella", False),
    "Altered immunity disorders": (["hiv-and-immunodeficiency", "autoimmune-disorders", "immunosuppressant-therapy"], r"lupus|\bhiv\b|anaphyla|rheumatoid|immunodefic|neutropen", False),
    "Med-Surg|Cardiac disorders": (["acute-coronary-syndrome", "cardiac-dysrhythmias", "vascular-disorders", "venous-thromboembolism", "inflammatory-and-valvular-heart-disease", "cardiovascular-diagnostics-and-monitoring"], r"myocardial infarction|stemi|endocarditis|pericarditis|aneurysm|deep vein thrombosis|pulmonary embol", False),
    "Endocrine disorders": (["renal-and-endocrine-diagnostics", "pituitary-disorders", "adrenal-disorders"], r"addison|cushing|siadh|diabetes insipidus|pheochromocytoma|thyroid storm", False),
    "Gastrointestinal disorders": (["liver-disorders", "pancreas-and-gallbladder-disorders", "bowel-disorders", "gi-diagnostic-procedures", "ostomy-care"], r"cirrho|pancreatit|cholecyst|bowel obstruction|crohn|colitis|diverticul|ostomy|hepatitis", False),
    "Hemodynamic monitoring": ([], r"arterial line|central venous pressure|\bcvp\b|pulmonary artery|wedge pressure|hemodynamic", False),
    "Musculoskeletal disorders": (["fracture-complications", "cast-and-traction-care", "joint-replacement-care", "arthritis-and-gout", "amputation-care", "musculoskeletal-medications"], None, False),
    "Med-Surg|Neurologic disorders": (["stroke-care", "seizure-management", "intracranial-pressure-and-head-injury", "degenerative-neurological-disorders", "meningitis-and-encephalitis", "neurological-assessment-and-procedures", "spinal-cord-injury"], r"stroke|seizure|intracranial|parkinson|multiple sclerosis|myasthenia|guillain|spinal cord", False),
    "Pacemaker": ([], r"pacemaker|pacing|implantable cardioverter|\bicd\b", False),
    "Pulmonary disorders": (["pneumonia-care", "chest-trauma-and-pneumothorax", "mechanical-ventilation", "tracheostomy-and-suctioning", "respiratory-diagnostic-procedures", "impaired-oxygenation", "oxygen-therapy-and-devices"], None, False),
    "Renal disorders": (["kidney-injury-and-disease", "dialysis-care", "urinary-tract-and-prostate-disorders"], r"kidney|renal|dialysis|creatinine clearance|nephr", False),
    "Sepsis": (["shock-and-sepsis"], r"sepsis|septic", False),
    "Sexually transmitted infections": ([], r"syphilis|gonorrh|chlamydia|genital herpes|trichomon|\bhpv\b|sexually transmitted|pelvic inflammatory", False),
    "Shock": (["shock-and-sepsis"], r"\bshock\b|hypovolem|vasopressor", False),
    "Trauma": (["burn-care", "intracranial-pressure-and-head-injury", "spinal-cord-injury"], r"trauma|motor vehicle|\bburns?\b|gunshot|stab wound|head injury", False),
    "Transplant rejection": (["organ-donation"], r"transplant|rejection|tacrolimus|cyclosporine|mycophenolate", False),
}


def match_rule(section, subtopic):
    """The matching rule for a subtopic: keyed by "Section|Subtopic" where a name appears in two sections."""
    return MATCH.get(f"{section}|{subtopic}") or MATCH.get(subtopic)


def check_match():
    """Every subtopic has a rule (a rule may be empty for case-only topics)."""
    missing = [f"{section}: {subtopic}" for section, subtopic, _ in BOOK if match_rule(section, subtopic) is None]
    if missing:
        raise SystemExit("book_topics.MATCH has no rule for:\n  " + "\n  ".join(missing))
