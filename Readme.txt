Hello and thanks for reviewing my entry for the Exploding Elves Challenge.

The game I created is not really meant to be interacted with but instead observed.
Instructions:
- Hold the left mouse button and drag to rotate your view
- Use the scroll bar to zoom in or out.
- You can press the + or - buttons to change the intervals at which each type of Kato spawns.
- The new interval will apply after the next spawn.
- There are 4 types of Katos that each spawn on their own island. Every Kato is slightly different even those of the same kind.
- The bigger Katos are slower but alot heavier and the smaller Katos are fast and light.
- If two Katos of the same type touch a new Kato will be created with the average traits of its parents.
- If two Katos of different types touch carnage will ensue.

I tried my best to optimize this game as much as I can. I took this chance to try out a Mesh Animator asset i have been meaning to experiment with that offloads animation overhead to the GPU.

The game does run at a very high frame rate but feel free to test it out for yourself. I also use object pooling and static batching to try and minimize CPU overhead where I can. My goal here was to create a modular scalable system that is efficient and ready to expand with more complexity.