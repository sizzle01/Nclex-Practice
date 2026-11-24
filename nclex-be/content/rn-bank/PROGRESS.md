# RN bank progress toward 5,000 questions

Written by `python3 -I build_all.py` (coverage.py); do not edit by hand.

**3034 of 5000 questions** (61%), 206 case studies, 63 bowtie questions. Multiple choice is 44% of the bank.

## By test plan category

| Category | Test plan | Target | Have | To write | Next chunk |
|---|---|---|---|---|---|
| Management of Care | 18% | 900 | 404 | 496 | 24 |
| Safety and Infection Prevention and Control | 13% | 650 | 295 | 355 | 17 |
| Health Promotion and Maintenance | 9% | 450 | 264 | 186 | 9 |
| Psychosocial Integrity | 9% | 450 | 252 | 198 | 10 |
| Basic Care and Comfort | 9% | 450 | 175 | 275 | 13 |
| Pharmacological and Parenteral Therapies | 16% | 800 | 536 | 264 | 13 |
| Reduction of Risk Potential | 12% | 600 | 299 | 301 | 14 |
| Physiological Adaptation | 14% | 700 | 809 | 0 | 0 |
| **Total** | 100% | 5000 | 3034 | 2075 | 100 |

## Chunks

| Chunk | Wave | Batch files | Questions |
|---|---|---|---|
| 1 | 1 | b68, b69, b70, b71 | 100 |
| 2 | 1 | b72, b73, b74, b75 | 100 |
| 3 | 1 | b76, b77, b78, b79 | 100 |
| 4 | 1 | b80, b81, b82, b83 | 100 |
| 5 | 1 | b84, b85, b86, b87 | 100 |
| 6 | 2 | b88, b89, b90, b91 | 100 |
| 7 | 2 | b92, b93, b94, b95 | 100 |
| 8 | 2 | b96, b97, b98, b99 | 100 |

The wave chunks are paused (2026-11-20) while the bank is rebuilt around case studies.

## Case conversion (CASE-MAP.md)

**1236 of 2376 active questions (52%) are inside full case studies**; the goal is 85%. 658 older questions are retired (F26, retired-questions.json): their topics were rebuilt as case studies.

| Chunk | Topic | Batch files | Questions | Case studies |
|---|---|---|---|---|
| 1 | Cardiac, pacemaker, hemodynamic monitoring and shock | b100, b101, b102, b103 | 100 | 16 |
| 2 | Pulmonary disorders with respiratory pharmacology | b104, b105, b106, b107 | 100 | 16 |
| 3 | Endocrine disorders with insulin, antidiabetic, thyroid and corticosteroid drugs | b108, b109, b110, b111 | 100 | 16 |
| 4 | Gastrointestinal disorders with PUD, GERD and antiemetic drugs | b112, b113, b114, b115 | 100 | 16 |
| 5 | Neurologic disorders, head-to-toe and cranial nerve assessment, neuro and cholinergic drugs | b116, b117, b118, b119 | 100 | 16 |
| 6 | Renal, hematologic and oncologic disorders with hematopoietic drugs and thrombolytics | b120, b121, b122, b123 | 100 | 16 |
| 7 | Immunity, infection, STI, musculoskeletal and trauma with antimicrobial, antifungal and antiparasitic drugs | b124, b125, b126, b127 | 100 | 16 |
| 8 | Mental health disorders with psychiatric medications | b128, b129, b130, b131 | 100 | 16 |
| 9 | Mental health part 2: abuse and violence, perinatal mood, dementia, alcohol, ECT, grief and remaining drugs | b132, b133, b134 | 76 | 12 |
| 10 | Maternity part 1: bleeding, diabetes, Rh, labor and fetal monitoring, prenatal care, newborn and postpartum | b135, b136, b137, b138 | 100 | 16 |
| 11 | Maternity part 2: hyperemesis, PPROM, HELLP, screening, intrapartum emergencies, newborn and postpartum | b139, b140, b141, b142 | 100 | 16 |

**Next: conversion chunk 12**: Pediatrics: growth and development, pediatric disorders by system and vaccines (CASE-MAP.md, book_topics.py).

## Review book checklist (book_topics.py)

**69 of 96 subtopics covered** (at least 2 case studies), 18 started (1 case), 9 not yet covered.

| Section | Subtopic | Status | Case studies |
|---|---|---|---|
| Fundamental | ABG | covered | Status asthmaticus in an adult; COPD with CO2 narcosis from too much oxygen; ARDS on mechanical ventilation; Euglycemic DKA on an SGLT2 inhibitor after surgery |
| Fundamental | ABO blood group | covered | Warfarin over-anticoagulation with a GI bleed and a mechanical valve; Lower GI bleed on apixaban; Disseminated intravascular coagulation from sepsis; Splenic laceration with hemorrhagic shock; Rh incompatibility and Rho(D) immune globulin; Pathologic newborn jaundice from ABO incompatibility |
| Fundamental | Basic EKG concept | covered | Drug-induced complete heart block and transcutaneous pacing; Ventricular tachycardia in a client with an ICD; Permanent pacemaker failure to capture; New atrial fibrillation with a rapid rate from thyrotoxicosis |
| Fundamental | Dosage calculation | covered | Heparin-induced thrombocytopenia with arterial thrombosis; Cardiogenic shock with a pulmonary artery catheter; Chemotherapy-induced nausea and vomiting; Acetaminophen overdose; Diabetic foot ulcer with osteomyelitis; Massive pulmonary embolism treated with alteplase; Severe falciparum malaria in a returning traveler; Preterm labor with tocolysis and magnesium; Postpartum hemorrhage needing second-line uterotonics |
| Fundamental | IV fluid | covered | Hyperosmolar hyperglycemic state; SIADH with a hyponatremic seizure |
| Fundamental | Electrolyte imbalances | covered | Heart failure with digoxin toxicity; SIADH with a hyponatremic seizure; Adrenal crisis in Addison disease; Refeeding syndrome on parenteral nutrition; Hematoma and hypocalcemia after thyroidectomy; Missed hemodialysis with hyperkalemia and fluid overload; Tumor lysis syndrome; TURP with bladder irrigation and TUR syndrome; Rhabdomyolysis after a long lie on the floor; Bulimia nervosa with severe hypokalemia |
| Fundamental | Lab | covered | Severe community-acquired pneumonia in an older adult; Acute cholangitis and ERCP; Acute hepatitis B and exposures; Small bowel obstruction with strangulation; Febrile neutropenia after chemotherapy; Tumor lysis syndrome; Lupus flare with lupus nephritis |
| Holistic health assessment | Head-to-toe assessment | covered | Head-to-toe assessment that uncovers a chronic subdural hematoma; Head-to-toe admission template in long-term care |
| Holistic health assessment | Head-to-toe assessment template | covered | Head-to-toe admission template in long-term care; Head-to-toe assessment that uncovers a chronic subdural hematoma |
| Holistic health assessment | Cranial nerve | covered | Cranial nerve assessment after craniotomy; Bell palsy and trigeminal neuralgia |
| Pharmacology | Antimicrobials: antibiotics | covered | Active pulmonary tuberculosis; Cystic fibrosis exacerbation; Infective endocarditis in a client who injects drugs; Severe C. difficile colitis and toxic megacolon; Pelvic inflammatory disease with a tubo-ovarian abscess; Necrotizing fasciitis of the leg; Secondary syphilis and a Jarisch-Herxheimer reaction; Rheumatoid arthritis biologic therapy with reactivated tuberculosis |
| Pharmacology | Antimicrobials: antiviral | covered | Influenza outbreak in long-term care; Advanced HIV with Pneumocystis pneumonia |
| Pharmacology | Antimicrobials: antifungal | started | Candidemia treated with antifungal drugs |
| Pharmacology | Antimicrobials: antiparasitic | started | Severe falciparum malaria in a returning traveler |
| Pharmacology | Antilipidemics | started | TIA, carotid endarterectomy and statin therapy |
| Pharmacology | Hematopoietic medications | covered | Anemia and mineral bone disease of chronic kidney disease; Febrile neutropenia after chemotherapy; Pernicious anemia compared with iron deficiency |
| Pharmacology | Cardiovascular: ANS overview | covered | Pheochromocytoma; Anaphylactic shock in a client taking a beta blocker |
| Pharmacology | Cardiovascular: adrenergic agonists | covered | Anaphylactic shock in a client taking a beta blocker; Cardiogenic shock with a pulmonary artery catheter |
| Pharmacology | Cardiovascular: adrenergic antagonists | covered | Pheochromocytoma; Type A aortic dissection |
| Pharmacology | Cardiovascular: PSNS overview and cholinergic medications | covered | Myasthenic crisis versus cholinergic crisis; Delirium on top of dementia |
| Pharmacology | Cardiovascular: anticholinergic | covered | Drug-induced complete heart block and transcutaneous pacing; Over-the-counter cold medicines in an older adult |
| Pharmacology | Cardiovascular: hypertension | started | Hypertensive emergency after stopping clonidine |
| Pharmacology | Cardiovascular: heart failure | covered | Heart failure with digoxin toxicity; Acute pulmonary edema in heart failure |
| Pharmacology | Cardiovascular: antidysrhythmic | covered | Ventricular tachycardia in a client with an ICD; New atrial fibrillation with a rapid rate from thyrotoxicosis |
| Pharmacology | Cardiovascular: angina | started | NSTEMI with a retroperitoneal bleed after catheterization |
| Pharmacology | Cardiovascular: fluid volume excess | covered | Acute pulmonary edema in heart failure; Ascites, paracentesis and spontaneous bacterial peritonitis |
| Pharmacology | Coagulation: anticoagulants | covered | Heparin-induced thrombocytopenia with arterial thrombosis; Warfarin over-anticoagulation with a GI bleed and a mechanical valve; Lower GI bleed on apixaban; Massive pulmonary embolism treated with alteplase |
| Pharmacology | Coagulation: antiplatelets | covered | NSTEMI with a retroperitoneal bleed after catheterization; TIA, carotid endarterectomy and statin therapy |
| Pharmacology | Coagulation: thrombolytics | covered | Acute ischemic stroke in the thrombolytic window; Massive pulmonary embolism treated with alteplase |
| Pharmacology | Pain, fever and inflammation | covered | Acetaminophen overdose; Opioid-induced respiratory depression in sleep apnea; Acute gout flare and urate-lowering therapy; Below-knee amputation with phantom limb pain; Opioid overdose and buprenorphine induction |
| Pharmacology | Endocrine: thyroid and antithyroid | covered | Thyroid storm; Myxedema coma; Levothyroxine over-replacement in an older adult |
| Pharmacology | Endocrine: osteoporosis | started | Osteoporosis with a vertebral compression fracture |
| Pharmacology | Endocrine: antidiabetic | covered | Newly diagnosed type 2 diabetes; Prolonged hypoglycemia from a sulfonylurea |
| Pharmacology | Endocrine: insulin | covered | Hyperosmolar hyperglycemic state; Insulin pump failure leading to DKA; Euglycemic DKA on an SGLT2 inhibitor after surgery |
| Pharmacology | GI: PUD and GERD | started | Perforated duodenal ulcer |
| Pharmacology | GI: antiemetics | started | Chemotherapy-induced nausea and vomiting |
| Pharmacology | Neurologic | covered | Status epilepticus after missed antiseizure doses; Parkinson disease after hip surgery; New-onset epilepsy and antiseizure drug safety; Multiple sclerosis relapse |
| Pharmacology | Respiratory: asthma and bronchoconstrictive disorders | covered | Status asthmaticus in an adult; COPD with CO2 narcosis from too much oxygen |
| Pharmacology | Respiratory: allergic disorders | started | Over-the-counter cold medicines in an older adult |
| Pharmacology | Respiratory: decongestant, antitussive, expectorant, mucolytic | started | Over-the-counter cold medicines in an older adult |
| Mental health | Anxiety disorders and OCD | covered | Panic disorder with anxiolytic treatment; Obsessive-compulsive disorder with exposure and response prevention; Generalized anxiety disorder with SSRI discontinuation and buspirone |
| Mental health | Bipolar mood disorders | covered | Bipolar mania with lithium toxicity; Bipolar disorder on valproate and lamotrigine with a severe rash; Postpartum psychosis |
| Mental health | Childhood and neurodevelopmental disorders | started | Child with ADHD on a stimulant |
| Mental health | Defense mechanisms | started | Alcohol use disorder with Wernicke encephalopathy |
| Mental health | Eating disorders | covered | Anorexia nervosa with medical instability; Bulimia nervosa with severe hypokalemia |
| Mental health | Impulse control disorders | started | Escalating aggression with seclusion and restraint |
| Mental health | Major depressive disorders | covered | Major depression with suicide risk early in SSRI treatment; Serotonin syndrome from fluoxetine with tramadol; Hypertensive crisis on an MAOI; Electroconvulsive therapy for catatonic depression; Complicated grief in an older widower |
| Mental health | Personality disorders | started | Borderline personality disorder with self-harm and splitting |
| Mental health | Schizophrenia spectrum disorders | covered | Schizophrenia with neuroleptic malignant syndrome; Treatment-resistant schizophrenia on clozapine; Olanzapine with metabolic syndrome and tardive dyskinesia |
| Mental health | Somatic symptom disorders | started | Functional neurological symptom disorder after a traumatic event |
| Mental health | Substance use disorders | covered | Alcohol withdrawal after orthopedic surgery; Opioid overdose and buprenorphine induction; Alcohol use disorder with Wernicke encephalopathy; Smoking cessation with varenicline; Neonatal opioid withdrawal syndrome |
| Mental health | Trauma, stressor-related and dissociative disorders | covered | Post-traumatic stress disorder in a veteran; Functional neurological symptom disorder after a traumatic event; Intimate partner violence in pregnancy; Sexual assault with trauma-informed forensic care |
| Mental health | Medications: anxiolytics | covered | Panic disorder with anxiolytic treatment; Post-traumatic stress disorder in a veteran; Generalized anxiety disorder with SSRI discontinuation and buspirone |
| Mental health | Medications: depression and mood stabilization | covered | Major depression with suicide risk early in SSRI treatment; Bipolar mania with lithium toxicity; Serotonin syndrome from fluoxetine with tramadol; Hypertensive crisis on an MAOI; Bipolar disorder on valproate and lamotrigine with a severe rash; Generalized anxiety disorder with SSRI discontinuation and buspirone; Smoking cessation with varenicline |
| Mental health | Medications: psychotic disorders | covered | Schizophrenia with neuroleptic malignant syndrome; Treatment-resistant schizophrenia on clozapine; Escalating aggression with seclusion and restraint; Olanzapine with metabolic syndrome and tardive dyskinesia |
| Maternity | Abruptio placentae | covered | Placental abruption after cocaine use; Placental abruption after a car crash |
| Maternity | Electronic fetal heart rate monitoring | covered | Oxytocin induction with tachysystole and late decelerations; Umbilical cord prolapse; Fetal growth restriction found by fundal height; Uterine rupture during a trial of labor after cesarean; Placental abruption after a car crash |
| Maternity | Fetal growth and development | covered | Fetal growth restriction found by fundal height; First prenatal visit with pregnancy terminology; Prenatal screening after an elevated AFP |
| Maternity | Signs of pregnancy | covered | First prenatal visit with pregnancy terminology; Third-trimester visit with expected discomforts |
| Maternity | Measuring fundal height | covered | Fetal growth restriction found by fundal height; First prenatal visit with pregnancy terminology; Third-trimester visit with expected discomforts; Gestational diabetes on insulin with night-time hypoglycemia |
| Maternity | Gestational diabetes mellitus | covered | Gestational diabetes from screening to birth; Gestational diabetes on insulin with night-time hypoglycemia; Shoulder dystocia with a macrosomic baby |
| Maternity | Labor and delivery | covered | Labor epidural with hypotension and the stages of labor; Umbilical cord prolapse; Oxytocin induction with tachysystole and late decelerations; Precipitous birth with the nurse alone; Shoulder dystocia with a macrosomic baby; Uterine rupture during a trial of labor after cesarean; Amniotic fluid embolism with cardiac arrest |
| Maternity | Medications used during labor and delivery | covered | Postpartum hemorrhage needing second-line uterotonics; Preterm labor with tocolysis and magnesium; Oxytocin induction with tachysystole and late decelerations; Labor epidural with hypotension and the stages of labor; HELLP syndrome with liver hematoma; Preterm prelabor rupture of membranes with chorioamnionitis |
| Maternity | Newborn | covered | Newborn assessment and routine medications; Pathologic newborn jaundice from ABO incompatibility; Newborn discharge exam and teaching; Late-preterm newborn with transient tachypnea and hypoglycemia |
| Maternity | Newborn complications | covered | Pathologic newborn jaundice from ABO incompatibility; Neonatal opioid withdrawal syndrome; Gestational diabetes from screening to birth; Meconium aspiration in a post-term newborn; Late-preterm newborn with transient tachypnea and hypoglycemia |
| Maternity | Placenta previa | started | Placenta previa with accreta |
| Maternity | Postpartum | covered | Postpartum hemorrhage from uterine atony; Postpartum endometritis and mastitis; Postpartum hemorrhage needing second-line uterotonics; Postpartum vulvar hematoma with postpartum vaccines |
| Maternity | Preeclampsia | covered | Preeclampsia with severe features at 35 weeks; HELLP syndrome with liver hematoma |
| Maternity | Pregnancy terminology | covered | First prenatal visit with pregnancy terminology; Third-trimester visit with expected discomforts |
| Maternity | Rh incompatibility | covered | Rh incompatibility and Rho(D) immune globulin; Rh-sensitized pregnancy with fetal anemia |
| Pediatrics | Growth and development: infant | **not yet** | - |
| Pediatrics | Growth and development: toddler | **not yet** | - |
| Pediatrics | Growth and development: preschool | **not yet** | - |
| Pediatrics | Growth and development: school age | **not yet** | - |
| Pediatrics | Growth and development: adolescence | **not yet** | - |
| Pediatrics | Cardiac disorders | **not yet** | - |
| Pediatrics | Genitourinary disorders | **not yet** | - |
| Pediatrics | Hematologic disorders | started | Sickle cell crisis with acute chest syndrome |
| Pediatrics | Neurologic disorders | **not yet** | - |
| Pediatrics | Respiratory disorders | covered | Infant with RSV bronchiolitis; Child with a severe asthma exacerbation |
| Pediatrics | Vaccination schedule | **not yet** | - |
| Med-Surg | Altered immunity disorders | covered | Anaphylactic shock in a client taking a beta blocker; Crohn disease flare and infliximab; Advanced HIV with Pneumocystis pneumonia; Lupus flare with lupus nephritis; Rheumatoid arthritis biologic therapy with reactivated tuberculosis; Candidemia treated with antifungal drugs |
| Med-Surg | Cardiac disorders | covered | Inferior ST-elevation myocardial infarction; Type A aortic dissection; Cardiac tamponade after coronary bypass surgery |
| Med-Surg | Endocrine disorders | covered | Adrenal crisis in Addison disease; Diabetes insipidus after pituitary surgery |
| Med-Surg | Gastrointestinal disorders | covered | Hepatic encephalopathy in cirrhosis; Severe acute pancreatitis |
| Med-Surg | Hemodynamic monitoring | covered | Cardiogenic shock with a pulmonary artery catheter; Arterial line and CVP monitoring after open AAA repair |
| Med-Surg | Musculoskeletal disorders | covered | Hemorrhage after a total hip replacement; Acute limb ischemia and compartment syndrome; Femur fracture in traction with fat embolism syndrome; Acute gout flare and urate-lowering therapy; Below-knee amputation with phantom limb pain; Rhabdomyolysis after a long lie on the floor |
| Med-Surg | Neurologic disorders | covered | Rising intracranial pressure after a head injury; Acute ischemic stroke; Myasthenic crisis versus cholinergic crisis; Guillain-Barre syndrome with respiratory failure; Intracerebral hemorrhage on warfarin; Bacterial meningitis with meningococcal sepsis; Parkinson disease after hip surgery; Multiple sclerosis relapse; ALS and goals of care |
| Med-Surg | Pacemaker | covered | Permanent pacemaker failure to capture; Drug-induced complete heart block and transcutaneous pacing |
| Med-Surg | Pulmonary disorders | covered | ARDS on mechanical ventilation; Active pulmonary tuberculosis |
| Med-Surg | Renal disorders | covered | Acute kidney injury with hyperkalemia; Missed hemodialysis with hyperkalemia and fluid overload; Peritonitis on peritoneal dialysis; Acute kidney injury from an NSAID, ACE inhibitor and diuretic; Infected obstructing kidney stone with urosepsis; TURP with bladder irrigation and TUR syndrome; Kidney transplant rejection and tacrolimus toxicity; Anemia and mineral bone disease of chronic kidney disease; Starting hemodialysis with disequilibrium syndrome |
| Med-Surg | Sepsis | covered | Older adult with sepsis from a urinary tract infection; Septic shock from an anastomotic leak after colectomy; Infected obstructing kidney stone with urosepsis; Disseminated intravascular coagulation from sepsis; Febrile neutropenia after chemotherapy; Necrotizing fasciitis of the leg; Candidemia treated with antifungal drugs |
| Med-Surg | Sexually transmitted infections | covered | Pelvic inflammatory disease with a tubo-ovarian abscess; Secondary syphilis and a Jarisch-Herxheimer reaction |
| Med-Surg | Shock | covered | Cardiogenic shock with a pulmonary artery catheter; Anaphylactic shock in a client taking a beta blocker; Cervical spinal cord injury, neurogenic shock and autonomic dysreflexia; Splenic laceration with hemorrhagic shock |
| Med-Surg | Trauma | covered | Flail chest with pulmonary contusion; Tension pneumothorax after a car crash; Major burn with inhalation injury; Epidural hematoma with a lucid interval; Cervical spinal cord injury, neurogenic shock and autonomic dysreflexia; Splenic laceration with hemorrhagic shock; Exertional heat stroke; Accidental hypothermia with rewarming; Mass-casualty incident with START triage; Femur fracture in traction with fat embolism syndrome; Placental abruption after a car crash |
| Med-Surg | Transplant rejection | started | Kidney transplant rejection and tacrolimus toxicity |

## Item types in the whole bank

| Type | Questions | Share |
|---|---|---|
| multiple_choice | 1343 | 44% |
| matrix_mc | 504 | 17% |
| sata | 403 | 13% |
| dropdown_cloze | 213 | 7% |
| highlight_table | 189 | 6% |
| highlight_text | 71 | 2% |
| ordered_response | 64 | 2% |
| bowtie | 63 | 2% |
| fill_blank_calc | 62 | 2% |
| dropdown_table | 43 | 1% |
| multiple_response_grouping | 29 | 1% |
| select_n | 25 | 1% |
| matrix_mr | 25 | 1% |
