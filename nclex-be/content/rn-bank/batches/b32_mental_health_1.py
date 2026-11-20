"""Batch 32: Mental health I - therapeutic communication, anxiety and trauma, crisis, grief, violence.

Rewritten to NCLEX-RN difficulty on 20 November 2026 (see quality.py)."""
from qb import *

add_subtopics({
    "therapeutic-communication": ("PSI", "Therapeutic communication"),
    "anxiety-and-related-disorders": ("PSI", "Anxiety, OCD and trauma-related disorders"),
    "crisis-intervention": ("PSI", "Crisis intervention"),
    "defense-mechanisms": ("PSI", "Defense mechanisms"),
    "grief-and-loss": ("PSI", "Grief and loss"),
    "cultural-and-spiritual-care": ("PSI", "Cultural and spiritual care"),
    "intimate-partner-and-sexual-violence": ("PSI", "Intimate partner and sexual violence"),
})

MH = "mental-health"
PSI = "PSI"
TC = "therapeutic-communication"
ANX = "anxiety-and-related-disorders"
CR = "crisis-intervention"
DM = "defense-mechanisms"
GR = "grief-and-loss"
CUL = "cultural-and-spiritual-care"
IPV = "intimate-partner-and-sexual-violence"

items = [
mc(M(PSI, TC, "take_action", "difficult", MH, "psych"),
   "A client admitted with major depression has been sitting alone in the dayroom. When the nurse sits nearby, the client says quietly, \"I'm worthless. Nobody would even notice if I was gone.\" Which response by the nurse is most therapeutic?",
   [("\"Are you having thoughts of ending your life?\"", True, "The statement hints at suicide; asking directly assesses safety and does not plant the idea."),
    ("\"You feel that you have no value to anyone right now?\"", False, "Reflecting is therapeutic, but the hint of suicide must be assessed directly first."),
    ("\"Your family visited yesterday, so I know they would notice.\"", False, "This argues with the feeling and misses the safety cue."),
    ("\"Why do you feel worthless when you have so much going for you?\"", False, "'Why' questions feel judgmental and this one minimizes the feeling.")],
   "Statements such as \"nobody would notice if I was gone\" are suicide cues; ask directly about suicidal thoughts.",
   "Use a therapeutic response."),

mc(M(PSI, TC, "evaluate_outcomes", "easy", MH, "psych"),
   "A nursing student is talking with a client who is anxious about a new diagnosis of breast cancer. The nurse is evaluating the student's communication. Which statement by the student is a nontherapeutic technique?",
   [("\"Tell me more about what the doctor said.\"", False, "This is an open-ended prompt."),
    ("\"Don't worry. They caught it early, so you'll be fine.\"", True, "False reassurance blocks discussion of feelings."),
    ("\"You seem worried since you came back from the clinic.\"", False, "Making an observation is therapeutic."),
    ("\"What has been the hardest part of today?\"", False, "This encourages description.")],
   "Avoid false reassurance, advice-giving, 'why' questions, changing the subject and judgment.",
   "Identify nontherapeutic communication."),

mc(M(PSI, TC, "take_action", "moderate", MH, "psych"),
   "During the third one-to-one session on an inpatient psychiatric unit, a client smiles and asks the nurse, \"Are you married? Do you have kids? Where do you live?\" What is the best response?",
   [("\"Our time is for you. How has your day been?\"", True, "This redirects to the client and keeps professional boundaries without rejecting the client."),
    ("\"I'm married with two kids, and I live in the next town over.\"", False, "Personal details shift the focus to the nurse and blur boundaries."),
    ("\"That's personal, and I'm not going to answer that.\"", False, "This is abrupt and damages rapport."),
    ("Pause, say nothing and wait for the client to change topics", False, "Ignoring the question is not therapeutic.")],
   "Keep the therapeutic relationship focused on the client.",
   "Maintain professional boundaries."),

mc(M(PSI, TC, "take_action", "difficult", MH, "psych"),
   "A 16-year-old admitted after an overdose says to the nurse, \"I'll tell you something really important, but only if you promise not to tell my parents or anyone else.\" What is the best response?",
   [("\"I can't promise that; I must share anything that affects your safety.\"", True, "Nurses cannot keep secrets that involve safety, such as suicidal plans or abuse; saying so up front keeps trust."),
    ("\"I promise I won't tell your parents, so you can trust me with it.\"", False, "The nurse may have to break this promise."),
    ("\"If you don't want it shared, it's better not to tell me at all.\"", False, "This shuts down communication about possible danger."),
    ("\"I'll keep it from your parents, but I'll tell your doctor.\"", False, "The nurse cannot promise what will be shared before hearing it.")],
   "Information that affects safety must be shared with the treatment team; say so before the client discloses.",
   "Respond to a request for secrecy."),

mc(M(PSI, ANX, "take_action", "difficult", MH, "psych"),
   "A client in the emergency department is breathing rapidly, trembling and clutching the chest, saying, \"I'm going to die.\" The ECG and troponin were normal 1 hour ago, and the SpO2 is 99%. The client has had two similar episodes this month. What should the nurse do first?",
   [("Stay with the client and give short, calm directions", True, "In panic, the client cannot process complex information; a calm presence lowers anxiety."),
    ("Explain that the tests were normal and the heart is not affected", False, "Detailed explanations cannot be processed during panic."),
    ("Have the client breathe into a paper bag to slow the breathing", False, "Paper bag rebreathing is not recommended and can cause hypoxia."),
    ("Leave the client in a quiet room to calm down alone", False, "A client in panic should not be left alone.")],
   "Severe or panic anxiety: stay, stay calm, simple directions, reduce stimulation.",
   "Respond to a panic attack."),

mc(M("PHI.PPT", "psychiatric-medications", "evaluate_outcomes", "moderate", MH, "ph"),
   "A client with generalized anxiety disorder who works as a forklift driver is starting buspirone 7.5 mg twice daily. The client used to take lorazepam as needed. Which statement shows correct understanding?",
   [("\"It may take a few weeks before I notice a difference.\"", True, "Buspirone's effects build over 2 to 4 weeks."),
    ("\"I'll take a dose whenever I start to feel anxious at work.\"", False, "Buspirone is taken on a schedule, not as needed."),
    ("\"It will make me sleepy, so I'll stop driving the forklift.\"", False, "It is minimally sedating, though effects should be checked."),
    ("\"I'll need to taper off slowly to avoid getting hooked.\"", False, "It has low dependence potential.")],
   "Buspirone: scheduled dosing, delayed effect, little sedation or dependence.",
   "Teach about buspirone."),

mc(M("PHI.PPT", "psychiatric-medications", "evaluate_outcomes", "moderate", MH, "ph"),
   "A client has taken lorazepam 1 mg three times daily for 6 weeks after a traumatic event and is starting therapy. The nurse reviews teaching about the medicine. Which statement by the client shows a need for more teaching?",
   [("\"I won't have wine with dinner while I'm on this.\"", False, "Alcohol adds to CNS depression."),
    ("\"Once I feel better, I'll just stop taking it.\"", True, "Stopping suddenly after regular use can cause withdrawal and seizures."),
    ("\"I won't drive until I know how it affects me.\"", False, "It causes drowsiness."),
    ("\"My provider will lower the dose slowly over weeks.\"", False, "Tapering is correct.")],
   "Benzodiazepines: avoid alcohol, taper off, watch for sedation and dependence.",
   "Teach safe benzodiazepine use."),

mc(M(PSI, ANX, "take_action", "difficult", MH, "psych"),
   "A client admitted yesterday with obsessive-compulsive disorder washes the hands for 30 minutes before every meal and arrives after the dining room closes. The hands are red and cracked. Staff ask how to handle mealtimes. What should the nurse do first?",
   [("Wake the client early so the ritual can be done before meals", True, "Interrupting rituals early raises anxiety; allow time now and limit gradually later."),
    ("Lock the bathroom before meals so the client cannot wash", False, "Blocking the ritual abruptly sharply increases anxiety."),
    ("Hold the meal tray until the client can arrive at the dining room on time", False, "This is punitive and risks poor nutrition."),
    ("Explain that the hands are clean and washing is not needed", False, "The client knows the ritual is excessive; logic does not reduce the anxiety.")],
   "OCD: early on, allow time for rituals and protect the skin, then gradually limit them while teaching coping skills.",
   "Plan care for a client with OCD."),

mc(M(PSI, ANX, "recognize_cues", "moderate", MH, "psych"),
   "A 29-year-old veteran returned from deployment 6 months ago. The client startles at loud noises, has nightmares about a roadside explosion, avoids driving on highways and says, \"I feel numb around my family.\" Which disorder do these findings suggest?",
   [("Post-traumatic stress disorder", True, "Hyperarousal, re-experiencing, avoidance and numbing after trauma are typical of PTSD."),
    ("Generalized anxiety disorder", False, "GAD is broad worry that is not tied to a trauma."),
    ("Acute stress disorder", False, "This lasts up to 1 month after the trauma."),
    ("Major depressive disorder", False, "Numbness can occur, but the trauma-linked symptoms point to PTSD.")],
   "PTSD: intrusive memories, avoidance, negative mood and hyperarousal for more than 1 month; use trauma-informed care.",
   "Recognize PTSD."),

matrix_mc(M(PSI, DM, "analyze_cues", "difficult", MH, "psych"),
   "The nurse is reviewing notes from group therapy. For each statement, indicate the defense mechanism it shows.",
   ["Denial", "Projection", "Displacement", "Rationalization"],
   [("A client newly diagnosed with diabetes keeps eating sweets and says, \"The lab must have mixed up my blood.\"", 0, ["Refusing to accept reality is denial.", "No feelings are attributed to others.", "No feelings are redirected.", "No excuse is offered for a behavior."]),
    ("A client who is angry at being admitted says to the nurse, \"You're the one who's angry with me.\"", 1, ["The reality is not refused.", "The client attributes their own feelings to the nurse.", "The feeling is not shifted to a safer target.", "No excuse is offered."]),
    ("After being criticized by the unit manager, a client slams a door and shouts at a roommate", 2, ["This is not denial.", "This is not projection.", "Anger is shifted to a safer target.", "This is not rationalization."]),
    ("A client who was fired for being late says, \"That job was going nowhere anyway.\"", 3, ["This is not denial.", "This is not projection.", "This is not displacement.", "A plausible excuse is given for the loss."])],
   "Recognize common defense mechanisms in client statements.",
   "Identify defense mechanisms."),

mc(M(PSI, CR, "prioritize_hypotheses", "difficult", MH, "psych"),
   "A client comes to the crisis center the day after losing a job and being evicted. The client is tearful, has not eaten since yesterday and says, \"I don't know where to go. I don't see the point anymore.\" What should the nurse assess first?",
   [("Whether the client has thoughts of suicide", True, "\"I don't see the point\" is a suicide cue; safety is the first priority in crisis."),
    ("Where the client can sleep tonight", False, "Shelter matters, but safety from self-harm comes first."),
    ("What helped the client cope with past losses", False, "Past coping is assessed after safety."),
    ("When the client last ate and drank", False, "Nutrition is a need, but not the most urgent.")],
   "Crisis intervention: ensure safety, focus on the immediate problem, use coping strategies and supports.",
   "Prioritize safety in crisis intervention."),

mc(M(PSI, GR, "analyze_cues", "easy", MH, "psych"),
   "A client with metastatic pancreatic cancer tells the hospice nurse, \"If I can just live until my daughter's wedding in June, I'll be ready to go.\" Which stage of grief does this reflect?",
   [("Bargaining", True, "Bargaining is trying to make a deal for more time."),
    ("Denial", False, "The client acknowledges the illness and death."),
    ("Acceptance", False, "The client is still negotiating for time."),
    ("Depression", False, "No sadness or withdrawal is described.")],
   "Kübler-Ross stages: denial, anger, bargaining, depression, acceptance (not always in order).",
   "Identify stages of grief."),

mc(M(PSI, GR, "analyze_cues", "moderate", MH, "psych"),
   "Two weeks after the sudden death of a spouse, a client cries often, has trouble sleeping and sometimes thinks they hear the spouse's voice in the house. The client eats regular meals and plans to go back to work next week. How should the nurse interpret these findings?",
   [("A normal grief response", True, "Sadness, sleep changes and briefly sensing the dead person are common early in grief."),
    ("Major depressive disorder", False, "It is too early to label this, and the client is functioning."),
    ("Prolonged grief disorder", False, "This is diagnosed when grief lasts 12 months or more and is disabling."),
    ("A psychotic disorder", False, "Briefly hearing a dead loved one's voice is a common part of grief.")],
   "Support normal grief; watch for prolonged, disabling grief or suicidal thoughts.",
   "Recognize normal grief."),

mc(M(PSI, CUL, "take_action", "moderate", MH, "fund"),
   "A client of Hmong heritage admitted with pneumonia asks for a shaman to perform a healing ceremony at the bedside in addition to IV antibiotics. The ceremony includes chanting and tying strings around the wrist. What is the nurse's best response?",
   [("Arrange a time for the ceremony and plan around the IV site", True, "Respecting cultural practices supports trust and adherence when they do not interfere with treatment."),
    ("Explain that hospital policy does not permit healing ceremonies", False, "This disregards cultural values without a safety reason."),
    ("Ask the client to choose between the ceremony and the antibiotics", False, "Both can be safely combined."),
    ("Allow the ceremony but remove the strings when the shift ends", False, "Removing the strings disrespects the practice.")],
   "Culturally responsive care includes safe traditional practices.",
   "Respect cultural healing practices."),

mc(M(PSI, CUL, "take_action", "easy", MH, "fund"),
   "A client hospitalized for 3 weeks with complications after surgery tells the nurse, \"I've prayed every day, and I feel like God has abandoned me.\" What should the nurse do?",
   [("Listen, explore the feeling and offer a chaplain visit", True, "This addresses spiritual distress respectfully and follows the client's wishes."),
    ("Change the subject to the plan for discharge home", False, "This dismisses the client's distress."),
    ("Share the nurse's own beliefs about faith and suffering", False, "This imposes the nurse's views."),
    ("Reassure the client that God has not abandoned anyone", False, "This argues with the client's experience.")],
   "Assess spiritual needs and refer according to the client's wishes.",
   "Respond to spiritual distress."),

mc(M(PSI, ANX, "recognize_cues", "difficult", MH, "psych"),
   "A client who witnessed a serious car crash yesterday suddenly cannot move the right arm. A CT scan, an MRI and nerve studies are normal. The client seems calm, eats with the left hand and says, \"I'm sure it will come back.\" Which condition do these findings suggest?",
   [("Conversion disorder linked to stress", True, "Neurologic symptoms without an organic cause, after stress and with apparent indifference, suggest conversion."),
    ("Acute ischemic stroke", False, "The imaging and nerve studies are normal."),
    ("Malingering for financial gain", False, "In conversion, symptoms are not produced on purpose."),
    ("Illness anxiety disorder", False, "Clients with this disorder are very worried, not calm.")],
   "Conversion disorder: real symptoms without an organic cause, linked to stress; avoid focusing on the symptom.",
   "Recognize conversion disorder."),

mc(M(PSI, ANX, "generate_solutions", "moderate", MH, "psych"),
   "A 34-year-old was bitten by a dog as a child and now avoids walking in the neighborhood and visiting friends who own dogs. The client wants to be able to walk to work. Which treatment is most effective?",
   [("Gradual exposure to dogs paired with relaxation", True, "Systematic desensitization reduces fear step by step."),
    ("Avoiding dogs by changing the route to work", False, "Avoidance keeps the fear going."),
    ("Spending an hour in a room with several dogs", False, "Flooding without preparation and consent is not the first choice."),
    ("Daily alprazolam before leaving the house", False, "Benzodiazepines are not first-line treatment for phobias.")],
   "Phobias respond to exposure-based behavioral therapy.",
   "Identify treatment for phobias."),

mc(M(PSI, IPV, "take_action", "difficult", MH, "psych"),
   "During a private screening at a prenatal visit, a client says the partner hit them last night and has threatened to \"finish it\" if they leave. The partner is in the waiting room. What should the nurse do first?",
   [("Assess immediate safety and begin a safety plan", True, "Safety comes first, and the client decides on next steps; a threat to kill raises the risk."),
    ("Tell the client to leave the partner and go to a shelter today", False, "Leaving can be the most dangerous time; the client decides."),
    ("Ask the partner to come in and talk about the problem together", False, "This raises the danger to the client."),
    ("Document the disclosure and give the client a hotline number", False, "Resources help, but a safety assessment and plan come first.")],
   "IPV: screen privately, believe the client, assess safety, create a safety plan and give resources.",
   "Respond to disclosure of intimate partner violence."),

mc(M(PSI, IPV, "analyze_cues", "moderate", MH, "psych"),
   "A client says that after each beating, the partner cries, apologizes, brings flowers and promises it will never happen again. The client says, \"This time I think they really mean it.\" Which phase of the cycle of violence is the client describing?",
   [("Reconciliation (honeymoon) phase", True, "Remorse and affection follow the battering."),
    ("Tension-building phase", False, "This involves rising stress and minor incidents."),
    ("Acute battering phase", False, "This is the violent episode."),
    ("Recovery and separation phase", False, "This is not a phase of the cycle.")],
   "Cycle of violence: tension building, then acute battering, then honeymoon, then repeat.",
   "Identify the cycle of violence."),

mc(M(PSI, IPV, "take_action", "moderate", MH, "psych"),
   "A client calls the emergency department 1 hour after being sexually assaulted. The client is crying and says, \"I just want to wash it all off and go to bed.\" Which instruction should the nurse give?",
   [("\"Please don't shower or change before coming in.\"", True, "This preserves evidence for the forensic exam."),
    ("\"Take a shower first if it helps you feel calmer.\"", False, "Showering destroys evidence."),
    ("\"Put your clothes in the washer, then come in.\"", False, "Clothing holds evidence and should be brought in a paper bag."),
    ("\"Rest tonight and come in first thing in the morning.\"", False, "Prompt care is needed for evidence and prophylaxis.")],
   "Sexual assault: preserve evidence, give prompt care (ideally a SANE exam) and offer support and prophylaxis.",
   "Teach evidence preservation after sexual assault."),

select_n(M(PSI, TC, "take_action", "moderate", MH, "psych"),
   "An 82-year-old resident of a long-term care facility says to the nurse, \"My son never visits me anymore. I guess I'm just a burden now.\" Select the 2 most therapeutic responses.",
   [("\"You're feeling hurt that your son doesn't visit?\"", True, "Reflecting the feeling is therapeutic."),
    ("\"Tell me more about you and your son.\"", True, "An open-ended prompt encourages expression."),
    ("\"I'm sure he's just very busy with work these days.\"", False, "This is false reassurance and minimizes the feeling."),
    ("\"Why don't you give him a call this afternoon?\"", False, "This gives advice."),
    ("\"My mother used to say the same thing to me.\"", False, "This shifts the focus to the nurse.")],
   "Reflection and open-ended questions encourage clients to share feelings.",
   "Choose therapeutic responses."),
]

add_subtopics({"psychiatric-medications": ("PHI.PPT", "Psychiatric medications")})

BUILT = build(32, "mental-health-1", "Mental health I: communication, anxiety, crisis and grief", items, hardened=True)
