<p align="center">
<img src="EchoExtender.png" width="480" height="333">
</p>
<h2>Echo Extender - A mod to ease to process of inserting Echoes into custom regions </h2>
<h3>Dependencies:</h3>
<li>EnumExtender by PasteBee</li>
<li>CustomRegionSupport by Garrakx</li>
<h3>Adding an Echo</h3>
First, place a <code>GhostSpot</code> object in the room you wish for your Echo to spawn. Then navigate to your CRS region installation and in the region's folder, add a <code>echoConv.txt</code> file. The contents of this while will determine what the Echo says, for formatting see the Unique Data Pearl guide for CRS. (NOTE: The only exception here, is that you do not need an ID to start with like 0-##)
<h3>Echo Settings</h3>
In order to modify your echo add an <code>echoSettings.txt</code> file next to your conversation file, then paste the following into it:
<pre><code>#This tag determines whether the Echo only spawns on Hunter or not. The fade effect is still shown for the other slugcats
onlyhunter : true
#This tag determines whether the player must visit the location of the Echo first, before it appears
priming : true
#This tag determines the relative size of the Echo (1 = 100%)
size : 1
#These tags determine the minimum required karma and the minimum required karma cap for the Echo to spawn
minkarma : 0
minkarmacap : 0
</code></pre>
Modify the values according to how you want your Echo to behave. Delaying Echo spawn with minKarma and minKarmaCap may still cause the Echo's fade to play the first time around the player visits
