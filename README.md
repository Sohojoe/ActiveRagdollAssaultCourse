# ActiveRagdollAssaultCourse
Research into Assault Course for training Active Ragdolls (using MujocoUnity+ml_agents)

----

#### Contributors
* Joe Booth ([SohoJoe](https://github.com/Sohojoe))
* Jackson Booth ([JacksonJabba](https://github.com/jacksonJabba))

----

#### Download builds (Mac, Windows): [001 through 004-Walker](https://github.com/Sohojoe/ActiveRagdollAssaultCourse/releases/tag/0.004)


----

## AssaultCourse004-Walker
![AssaultCourse004-Walker](images/AssaultCourse004Walker.202-10m.gif)
* **Mujoco Model:** DeepMindWalker
* **Hypostheis:** Adversarial Trainer will scale to more complex models
* **Outcome:** Worked well.
* **Raw Notes:**
  * July 27 2018: Compared recurrent vs no recurrent over 1m training steps with two training runs each. **Recurrent was 25% improvement over non-recurrent** (recurrent score 440 & 424 so 432. Non-recurrent score 357 & 331 so 344) Note: is slower to train
  * Non recurrent vs with recurrent TODO ADD IMAGE
  * July 26 2018: Walker004.204 / 205 - train with recurrent as a comparison (205 is a sanity check)
  * Walker004.203 / 206 - train with no recurrent on terrainBrain for 1m steps
  * Walker004.202 - trained 8m, 10m
  * TODO - add image
  * July 25 2018: Walker004.202 - trained 5m
  * Walker004.202 - reverted double velocity
  * Walker004.201 - double velocity did not work out well
  * July 24 2018: Walker004 - made reward function double velocity reward after falling over,


## AssaultCourse004-Hopper
![AssaultCourse004-Hopper](images/AssaultCourse004.203.gif)
* **Mujoco Model:** DeepMindHopper
* **Hypostheis:** Dynamically adapting terrain will improve robustness and performance. Compare hand coded logic, vs random, vs Adversarial Trainer
* **Outcome:** Adversarial Trainer (using inverse reward from hopper brain) is less complex and signifcantly more effective.
* **Raw Notes:**
  * July 24 2018: 004.203 - use an adversarial network for the 2nd trainer - **worked really well!!**
  * TODO ADD image Compared to 202 Add image
  * July 23 2018: 004.202 - training for 5m steps (no difference in reward for up and down)
  * 004.16 - fixed some small issues with .15 logic
  * 004.15 - 0.25 pain if on knee or bum, 1 pain if head or noise. If pain, no upright bonus
  * 004.14 - fix, ‘Remove knee collision’ - was not working properly. Outcome: gets stuck on knee - try pain
  * July 22 2018: 004.13 - trying default learning rate - note: this worked (i.e. trained 1m steps without needing 100k first)
  * 004.12 Added ‘Spawn terrain further ahead so that it has more chance to prepare’
  * 004.11 - trying 100k then 1m steps worked. Not sure its much better than .09 (could try training for 5m steps, but will try extra gap
  * 004.10 - tried training to 1m steps and the guy just stands there.
  * Added ‘Remove knee collision’
  * Added ‘Higher reward for going upward’ - using 2x
  * 004.09 - is training pretty well, focuses on jumping down (as actor finds it harder to jump up) - tried 1m steps and 5m steps. Try
    * Remove knee collision
    * Spawn terrain further ahead so that it has more chance to prepare
    * Higher reward for going upward
  * July 21 004.01 - 


  

## AssaultCourse003
![AssaultCourse003](images/AssaultCourse003.gif)
* **Mujoco Model:** DeepMindHopper
* **Hypostheis:** Integrating perception of height will improve training
* **Outcome:** hopper is aware of and adapts to the terain.
* **References:** 
  * Insperation for observations is from DeepMind paper: [Emergence of Locomotion Behaviours in Rich Environments. arXiv:1707.02286 [cs.AI]](https://arxiv.org/abs/1707.02286) see: B Additional experimental details
* **Raw Notes:**
  * July 20: 003.33 - make height perception relative to foot (hypothesis: this will help it learn faster as heights will be relative) - **worked**
  * July 18: 003.32  uprightBonus *= .3f; - **failed**
  * 003.31  uprightBonus *= .3f; - **failed**
  * 003.30  normal reward for 1m steps
  * 003.29  no uprightBonus
  * 003.27  uprightBonus *= Mathf.Clamp(velocity,0,1);


## AssaultCourse002
* **Mujoco Model:** DeepMindHopper
* **Hypostheis:** Agent should be able to learn to traverse box objects **without** adding perception of height.
* **Outcome:** Did not work - Objects may have being too high. 
* **Raw Notes:**
  * July 4: Hop 8 - Tried blockyer obstacles to see their reaction. Hoppers couldn't get over obstacles, couldn't jump over them, will try with varying heights to see if that can teach them to jump over.

  
## AssaultCourse001
![AssaultCourse001](images/AssaultCourse001.gif)
* **Mujoco Model:** DeepMindHopper
* **Hypostheis:** Agent should be able to learn to traverse simple slopes **without** adding perception of height.
* **Outcome:** It worked - seams that it helps to have different slopes across the 16 agents. We needed a couple of iterations to get it right.
* **Raw Notes:**
  * July 4: Hop 6 - tried disabling curiosity to see if it was necessary for training hoppers, they were almost as effective as hop 5 but not quite although they trained significantly faster.
  * Hop 7 - With curiosity disabled I test the hoppers where some have obstacles and some don't. May make them faster while still retaining ability to maneuver obstacles. Just took longer to train, didn't have an effect on actual speed of hopper
  * Hop1.6 - tried using brain for hop 1 to see how it’d react and adapt to new obstacle environment, may lead to faster speeds. Wasn’t even slightly effective. The hoppers fell when they reached obstacles. 
  * July 3: Hop4 - first training with obstacles. Didn’t react well with environment, scooted over bumpy terrain and hills.
  * Hop5 - added longer flat stretch before obstacles, may lead to better results when facing difficult terrain. Was quite effective at difficult terrain although not as fast as hop 1 or 2.
