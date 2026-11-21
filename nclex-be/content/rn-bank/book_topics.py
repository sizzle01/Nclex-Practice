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
