<div align="center">

# DrunkStride

</div>

This project is the assignement of the course "AI for videogames" at the University of Milan.

Assignment date: 24/12/2023

Deadline: 18/01/2024

Delivery date: ?/01/2024  
<div align="center">

## Assignement text
</div>

### Goal
The goal of this project is to create an agent roaming on a platform while never moving along
a straight line.
### Setup
The platform can be of any size, square or rectangular.
The agent will change trajectory at random intervals. Each time the agent will travel over a
circumference leading first to right then to the left, then to the right again … and so on.
The agent is moving at a constant speed of 1 meter per second.
At each interval, the agent will pick a random value for the time to the next trajectory change
in the range (0, 10] seconds. Note that 0 is excluded.
Then, the agent selects a random circumference leading right or left with a radius between 0
(excluded) and the maximum radius that is not making the agent fall off the platform.
The selection of the radius is independent from the time of the next trajectory change.
The agent never stops moving.
### Constraints
The system must work independently on the platform shape or size.

<div align="center">

## Problem analysis
</div>

Summing up the assignement text, we can identify the following elements divided per category:
### Movement
<ul>
    <li>The agent must move on a rectangular or square platform of any size.</li>
    <li>The agent must move at a constant speed of 1 meter per second.</li>
    <li>The agent must change trajectory at random intervals in the [0: 10]\{0} range.</li>
    <li>The agent must move just along a circular line.</li>
    <li>The agent must move first alternatively to the right/left each time the new trajectory is computed.</li>
    <li>The agent must never stop moving.</li>
    <li>The agent must never fall off/move oustside the platform.</li>
</ul>

### Events
<ul>
	<li>The random intervals to compute new trajectories that must be mutually independent.</li>
</ul>

### Platform        
<ul>
    <li> The system must work independently on the platform shape or size. </li>
</ul>

<div align="center">

## Adopted solution
</div>

### Overview
The `Movement` class is a `MonoBehaviour` that controls the movement of a character in a 3D space. 
It uses a `CharacterController` to move the character along a circular path on a platform
which is a plane randomly instantiated at the start of the game.

### Environment
To satisfy the assignement, an agent and a platform are required. The majority of the code
has been developed in the `Movement.cs` script, described below according an high level analysis:

### Class Methods
- `Awake()`: here is instantiated a randomized plane working as the platform where the agent will move on 
thanks to the `InstantiateRandomizedPlane()` method, which also provides to compute the more intenal bounds
where all the new circular paths  will be computed, exploiting a slightly smaller plane than the original
one to prevent the agent from falling down or going outside the platform.

- `Start()`: initializes the collider of the capsule used as the agent and its position is placed 
to the center of the platform, then the `GetClosestAndFurthestVertex()` method calculates the closest
and furthest vertices to the agent for the first time. This method is useful to compute the maximum acceptable
radius for the new circular paths, which is the distance between the agent and the closest vertex.

- `Update()`: starts with the `changeInterval` variable update.
    Then, when the timer expires, the direction of the agent is changed through the boolean `isMovingRight`, followed by
    the retrieval of the new closest and furthest vertices.

    Moreover, the method `WhereIsAgent()` is launched to check on which
    side of the platform the agent is located, since the new circular path takes in account this information for its computation.
    Indeed, the new trajectory is calculated in the `SetCirclePosition()` method according to the results of the previous methods called 
    because one retrieves the maximum size of the new radius and the other the position of the new center of the circle.

    Below a clearer view of what described so far:

    <div align="center">

    ![Capsule Dirs](AgentScheme.jpg)

    *Figure 1: the computation of the new circle center position considers the platform's quadrant where the agent relies.*

    </div>

    To obtain the right circumference that will be the new path for the agent, the `SetCirclePosition()` method performs a check on
    how many tries have been occured to find a valid circle so that if reached the maximum number, the new circle remains the previous one, avoiding an infinite loop. 
    
    Otherwise, if the number of attempts is still acceptable, there is a control for the main points of the circle ( 0°, 90°, 180°, 270°), 
    specifically if they are all contained in the bounds defined in `InstantiateRandomizedPlane()` at the beginning of the script, only then the new circle is successfully found.

    At the end of the `if` statement which contains the actions described so far, there is the `SetNewTime()` method call
    that simply counts how many times the timer is expired and provides a new random interval for the next one.

# FromHereNextTime

- `SetNewChangeTime()`: Randomly sets the time interval for the character to change direction.
- `SetCirclePosition()`: Calculates a new circular path for the character.
- : Calculates the closest and furthest vertices of the platform from the character.
- `ChControllerMovement()`: Moves the character along the circular path.
- `DrawDebugRays()`: Draws debug rays in the Unity editor for visualizing the character's path.
- `WhereIsAgent()`: Determines the character's position relative to the center of the platform.
- : Instantiates a new randomized plane for the platform.
