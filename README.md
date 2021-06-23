<p align="center">
<img src="EchoExtender.png" width="480" height="333">
</p>
<h2>Echo Extender - A mod to ease to process of inserting Echoes into custom regions </h2>
<h3>Dependencies:</h3>
<li>EnumExtender by PasteBee</li>
<li>CustomRegionSupport by Garrakx</li>
<h3>Adding an Echo</h3>
First, place a <code>GhostSpot</code> object in the room you wish for your Echo to spawn. Navigate to <code>/Mods/CustomResources/YourMod/World/Regions/XX</code> and add a <code>echoConv.txt</code> file. The contents of this while will determine what the Echo says, for formatting see the Unique Data Pearl guide for CRS. (NOTE: The only exception here, is that you do not need an ID to start with like 0-##) Example:
<img src="https://media.discordapp.net/attachments/855877860254679080/857234268987457546/unknown.png">
<h3>Conditional Dialogue</h3>
Similarly to when adding creatures, adding tags like <code>(0)</code>, <code>(1,2)</code> and so on will make that line of dialogue only appear on a specific difficulty
<h3>Echo Settings</h3>
In order to modify your echo add an <code>echoSettings.txt</code> file next to your conversation file, then paste the following into it:
<pre><code>#This array of numbers determines what difficulties the echo will spawn on (0 = Survivor, 1 = Monk, 2 = Hunter)
difficulties : 0, 1, 2
#This tag determines whether the player must visit the location of the Echo first, before it appears
priming : false
#This tag determines the relative size of the Echo (1 = 100%)
size : 1
#These tags determine the minimum required karma and the minimum required karma cap for the Echo to spawn. Lowest karma value is 1
minkarma : 1
minkarmacap : 1
#This tag determines the approximate screen-radius of the Echo effect (meaning it only applies from the core room that many rooms away)
radius : 4
#This tag determines which song plays at the location of the echo. You can either specify another echo's region (ex.: CC) or the name of a track (ex.: NA_34 - Else3)
echosong : SH
</code></pre>
Modify the values according to how you want your Echo to behave. Delaying Echo spawn with minKarma and minKarmaCap may still cause the Echo's fade to play the first time around the player visits
