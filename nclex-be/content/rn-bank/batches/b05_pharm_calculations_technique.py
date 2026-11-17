"""Batch 5: Pharmacological and parenteral therapies II - dosage calculation and administration technique (PHI.PPT)."""
from qb import *

PPT = "PHI.PPT"
PH = "pharmacology"
CALC = "medication-dose-calculations"
TECH = "medication-administration-technique"

def c(diff, stem, unit, decimals, label, value, why, obj, body=PH):
    return calc(M(PPT, CALC, "take_action", diff, body, "ph"), stem, unit, decimals, label, value, why, obj)

items = [
c("easy", "The provider prescribes cephalexin 0.5 g PO every 6 hours. The tablets available are 250 mg each. How many tablets should the nurse give per dose?",
  "tablets", 0, "Tablets per dose", 2,
  "Convert: 0.5 g = 500 mg. Tablets: 500 mg ÷ 250 mg per tablet = 2 tablets.",
  "Convert grams to milligrams and calculate the number of tablets."),

c("easy", "The provider prescribes amoxicillin 400 mg PO. The suspension contains 250 mg/5 mL. How many mL should the nurse give?",
  "mL", 0, "Volume per dose", 8,
  "Volume = (dose wanted ÷ dose on hand) × volume = (400 ÷ 250) × 5 mL = 8 mL.",
  "Calculate a liquid oral dose."),

c("easy", "The provider prescribes ketorolac 15 mg IM. The vial contains 30 mg/mL. How many mL should the nurse give? Round to the nearest tenth.",
  "mL", 1, "Volume to inject", 0.5,
  "Volume = 15 mg ÷ 30 mg/mL = 0.5 mL.",
  "Calculate an injectable dose."),

c("moderate", "The provider prescribes 1,000 mL of lactated Ringer's solution over 8 hours. The tubing's drop factor is 15 gtt/mL. At how many drops per minute should the nurse set a gravity infusion? Round to the nearest whole number.",
  "gtt/min", 0, "Drip rate", 31,
  "gtt/min = (volume × drop factor) ÷ time in minutes = (1,000 × 15) ÷ 480 = 31.25, rounded to 31 gtt/min.",
  "Calculate a gravity drip rate."),

c("easy", "An IV piggyback of 100 mL is to infuse over 30 minutes. At what rate should the nurse set the pump?",
  "mL/hr", 0, "Infusion rate", 200,
  "mL/hr = 100 mL ÷ 0.5 hr = 200 mL/hr.",
  "Calculate a pump rate for a short infusion."),

c("difficult", "A child weighs 22 lb. The provider prescribes cefazolin 25 mg/kg/day IV, divided into doses every 8 hours. How many mg should the nurse give per dose? Round to the nearest tenth.",
  "mg", 1, "Dose", 83.3,
  "Weight: 22 lb ÷ 2.2 = 10 kg. Daily dose: 25 mg/kg × 10 kg = 250 mg/day. Every 8 hours = 3 doses: 250 ÷ 3 = 83.3 mg per dose.",
  "Calculate a divided weight-based pediatric dose.", body="pediatrics"),

c("easy", "The provider prescribes heparin 5,000 units subcutaneously. The vial contains 10,000 units/mL. How many mL should the nurse give? Round to the nearest tenth.",
  "mL", 1, "Volume to inject", 0.5,
  "Volume = 5,000 units ÷ 10,000 units/mL = 0.5 mL.",
  "Calculate a subcutaneous heparin dose."),

c("difficult", "A client who weighs 80 kg is prescribed dopamine at 5 mcg/kg/min. The bag contains dopamine 400 mg in 250 mL of D5W. At what rate should the nurse set the pump?",
  "mL/hr", 0, "Infusion rate", 15,
  "Dose: 5 mcg × 80 kg = 400 mcg/min × 60 = 24,000 mcg/hr. Concentration: 400 mg = 400,000 mcg ÷ 250 mL = 1,600 mcg/mL. Rate: 24,000 ÷ 1,600 = 15 mL/hr.",
  "Calculate a mcg/kg/min infusion rate."),

c("easy", "500 mL of 0.9% sodium chloride is infusing at 125 mL/hr. How many hours will the infusion take?",
  "hours", 0, "Infusion time", 4,
  "Time = volume ÷ rate = 500 mL ÷ 125 mL/hr = 4 hours.",
  "Calculate infusion time."),

c("moderate", "A client is to receive 50 mL/hr of D5W through microdrip tubing (60 gtt/mL). At how many drops per minute should the nurse set the infusion?",
  "gtt/min", 0, "Drip rate", 50,
  "gtt/min = (50 mL × 60 gtt/mL) ÷ 60 minutes = 50 gtt/min. With microdrip tubing, gtt/min equals mL/hr.",
  "Calculate a microdrip rate."),

c("difficult", "A 1 g vial of ceftriaxone is reconstituted with 2.4 mL of sterile water to make 350 mg/mL. The provider prescribes 500 mg IM. How many mL should the nurse give? Round to the nearest tenth.",
  "mL", 1, "Volume to inject", 1.4,
  "Volume = 500 mg ÷ 350 mg/mL = 1.43 mL, rounded to 1.4 mL.",
  "Calculate a dose from a reconstituted medication."),

c("moderate", "The provider prescribes potassium chloride 20 mEq in 100 mL of 0.9% sodium chloride to infuse over 2 hours. At what rate should the nurse set the pump?",
  "mL/hr", 0, "Infusion rate", 50,
  "Rate = 100 mL ÷ 2 hr = 50 mL/hr (10 mEq/hr, within the usual peripheral limit).",
  "Calculate an IV potassium infusion rate."),

mc(M(PPT, CALC, "analyze_cues", "difficult", "pediatrics", "ph peds"),
   "A child who weighs 15 kg is prescribed acetaminophen 200 mg PO every 6 hours as needed. The safe dose is 10 to 15 mg/kg/dose. What should the nurse do?",
   [("Give the dose, because it is within the safe range", True, "The safe range is 150 to 225 mg per dose (10 × 15 to 15 × 15), and 200 mg falls within it."),
    ("Hold the dose, because it is too high", False, "200 mg is below the 225 mg maximum."),
    ("Hold the dose, because it is too low to be effective", False, "200 mg is above the 150 mg minimum."),
    ("Give half the dose to be safe", False, "Changing the dose without an order isn't appropriate, and the dose is already safe.")],
   "Calculate the safe range (mg/kg × weight) and compare it with the prescribed dose before giving any pediatric medication.",
   "Verify a pediatric dose against the safe range."),

mc(M(PPT, TECH, "evaluate_outcomes", "moderate", "cardiovascular", "ph aha"),
   "A client with stable angina has been prescribed sublingual nitroglycerin. Which statement shows correct understanding of chest pain management?",
   [("\"I'll take up to three tablets, 5 minutes apart, before calling for help.\"", False, "Emergency services should be called if pain isn't better or is worse 5 minutes after the first dose."),
    ("\"If my pain isn't better 5 minutes after the first tablet, I'll call 911.\"", True, "Current guidance: call 911 if chest pain is unimproved or worse 5 minutes after one dose."),
    ("\"I'll swallow the tablet with water.\"", False, "Sublingual nitroglycerin dissolves under the tongue; swallowing it reduces its effect."),
    ("\"I'll keep the tablets in a plastic pill box in my car.\"", False, "Nitroglycerin is kept in its original dark glass bottle, away from heat and light.")],
   "Sit down, take one tablet under the tongue and call 911 if the pain is unimproved or worse after 5 minutes.",
   "Teach correct use of sublingual nitroglycerin."),

mc(M(PPT, TECH, "take_action", "easy", PH, "ph fund"),
   "A client is prescribed timolol eye drops and erythromycin eye ointment for the same eye at the same time. What should the nurse do?",
   [("Give the ointment first, then the drops", False, "Ointment forms a barrier that keeps drops from being absorbed."),
    ("Give the drops first, wait about 5 minutes, then give the ointment", True, "Drops go first so they are absorbed; then the ointment."),
    ("Mix the drops into the ointment", False, "Medications aren't mixed."),
    ("Give both at once", False, "Giving them together reduces the effect of the drops.")],
   "Give eye drops before ointment and wait about 5 minutes between different eye medications.",
   "Sequence eye medications correctly."),

mc(M(PPT, TECH, "take_action", "easy", "pediatrics", "peds"),
   "The nurse is giving ear drops to a 2-year-old. How should the nurse position the ear?",
   [("Pull the pinna down and back", True, "In children under 3, the ear canal points upward, so pulling the pinna down and back straightens it."),
    ("Pull the pinna up and back", False, "This is the technique for adults and children over 3."),
    ("Pull the earlobe forward", False, "This doesn't straighten the canal."),
    ("Don't move the pinna", False, "Straightening the canal helps the drops reach the eardrum.")],
   "Under 3 years: pull the pinna down and back. Adults: up and back.",
   "Give ear drops to a young child."),

mc(M(PPT, TECH, "evaluate_outcomes", "moderate", "respiratory", "ph"),
   "A client uses an albuterol inhaler and a fluticasone inhaler every morning. Which statement shows correct understanding?",
   [("\"I'll use the fluticasone first, then the albuterol.\"", False, "The bronchodilator should be used first to open the airways."),
    ("\"I'll use the albuterol first, wait a few minutes, then use the fluticasone and rinse my mouth.\"", True, "Albuterol opens the airways so the steroid reaches deeper, and rinsing prevents oral candidiasis."),
    ("\"I'll use fluticasone when I have an asthma attack.\"", False, "Inhaled steroids are for prevention; albuterol is the rescue inhaler."),
    ("\"I don't need to rinse my mouth after the fluticasone.\"", False, "Rinsing prevents thrush and hoarseness.")],
   "Use the bronchodilator before the inhaled corticosteroid, then rinse the mouth after the steroid.",
   "Teach the sequence for bronchodilator and corticosteroid inhalers."),

mc(M(PPT, TECH, "evaluate_outcomes", "moderate", "gastrointestinal", "ph fund"),
   "The nurse is observing a student give medications through a client's nasogastric feeding tube. Which action requires the nurse to intervene?",
   [("Checks tube placement before giving medications", False, "Placement is verified before anything is put into the tube."),
    ("Mixes all of the crushed medications together in one cup", True, "Each medication is given separately with a flush between them to prevent interactions and tube clogging."),
    ("Flushes the tube with 15 to 30 mL of water before and after the medications", False, "Flushing keeps the tube open and ensures the full dose is given."),
    ("Uses the liquid form of a medication when one is available", False, "Liquid forms are preferred for tube administration.")],
   "Give each medication separately, flushing with water between medications.",
   "Give medications safely through an enteral tube."),

mc(M(PPT, TECH, "take_action", "easy", "pediatrics", "peds"),
   "Which site should the nurse use to give an intramuscular vaccine to a 4-month-old infant?",
   [("Deltoid", False, "The infant deltoid is too small for most IM injections."),
    ("Vastus lateralis", True, "The vastus lateralis is well developed in infants and has no major nerves or vessels nearby."),
    ("Dorsogluteal", False, "The dorsogluteal site risks sciatic nerve injury and isn't used in infants."),
    ("Ventrogluteal", False, "This site is used in older children and adults; the vastus lateralis is preferred in infants.")],
   "In infants, the vastus lateralis is the preferred IM site.",
   "Select the IM site for an infant."),

mc(M(PPT, TECH, "take_action", "moderate", "respiratory", "fund"),
   "The nurse is giving a tuberculin skin test. Which technique is correct?",
   [("Inject at a 45-degree angle into the subcutaneous tissue", False, "This is a subcutaneous injection."),
    ("Inject at a 10- to 15-degree angle with the bevel up to form a small wheal", True, "The intradermal technique places the antigen just under the epidermis."),
    ("Inject at a 90-degree angle into the deltoid", False, "This is an IM injection."),
    ("Massage the site after the injection", False, "Massage can spread the antigen and affect the result.")],
   "Intradermal injection: a 10 to 15 degree angle, bevel up, forming a wheal. Read it in 48 to 72 hours.",
   "Perform an intradermal injection."),

mc(M(PPT, TECH, "take_action", "easy", PH, "fund"),
   "The nurse is measuring 15 mL of a liquid medication in a medicine cup. How should the nurse read the volume?",
   [("At the top edge of the curve of the liquid, holding the cup above eye level", False, "Reading from above or below eye level distorts the measurement."),
    ("At the bottom of the meniscus with the cup at eye level on a flat surface", True, "The bottom of the meniscus at eye level gives the accurate volume."),
    ("By holding the cup in the hand and estimating", False, "Holding the cup can tilt it."),
    ("By filling the cup to the brim", False, "This gives an inaccurate dose.")],
   "Read liquid measurements at the bottom of the meniscus, at eye level, on a flat surface.",
   "Measure liquid medications accurately."),

mc(M(PPT, TECH, "analyze_cues", "moderate", PH, "ph"),
   "A client has difficulty swallowing pills. Which medication may the nurse crush?",
   [("Metoprolol succinate extended-release", False, "Crushing an extended-release tablet releases the whole dose at once."),
    ("Enteric-coated aspirin", False, "Crushing destroys the coating that protects the stomach."),
    ("Acetaminophen 325 mg regular tablet", True, "Plain immediate-release tablets can usually be crushed."),
    ("Nifedipine extended-release", False, "Crushing an extended-release calcium channel blocker can cause severe hypotension.")],
   "Don't crush extended-release (ER, XL, SR, CR), enteric-coated or sublingual products. Check with the pharmacist or ask for a liquid form.",
   "Identify medications that must not be crushed."),

sata(M(PPT, TECH, "take_action", "moderate", "endocrine", "ph fund"),
   "The nurse is teaching a client how to self-inject insulin with a syringe. Which instructions should the nurse include?",
   [("Rotate injection sites within the same body area", True, "Rotating within one area prevents lipohypertrophy and keeps absorption consistent."),
    ("Inject at a 90-degree angle into a pinched skin fold", True, "This is the usual angle for subcutaneous insulin with short needles."),
    ("Roll cloudy insulin gently between the palms before drawing it up", True, "Rolling mixes the suspension without creating bubbles."),
    ("Shake the NPH vial vigorously", False, "Shaking creates bubbles and can damage the insulin."),
    ("Massage the site after injecting", False, "Massage can change how quickly the insulin is absorbed."),
    ("Keep the vial in use at room temperature for up to 28 days", True, "Opened vials can usually be kept at room temperature for about 28 days; check the product information.")],
   "Insulin teaching: correct site rotation, technique, mixing and storage.",
   "Teach self-injection of insulin."),

mc(M(PPT, TECH, "take_action", "moderate", PH, "fund"),
   "Before giving medications, when should the nurse compare the medication label with the medication administration record?",
   [("Only once, at the bedside", False, "One check doesn't give enough safety."),
    ("When taking the medication from storage, when preparing it and at the bedside before giving it", True, "Three checks give three chances to catch an error."),
    ("Only when the medication is new", False, "Every medication is checked every time."),
    ("After giving the medication, when documenting", False, "Checks after giving are too late to prevent an error.")],
   "Do three checks against the MAR: when retrieving, when preparing and at the bedside, along with the rights of medication administration.",
   "Apply the three checks of medication administration."),
]

BUILT = build(5, "pharm-calculations-technique", "Pharmacology: dosage calculation and administration technique", items)
