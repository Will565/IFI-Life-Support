IFI-Life-Support
================
Released Under GPL3 License.
 <IFI Life Support Mod>
    Copyright (C) 2014  Will Schramm

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>


I know that there are a few Life support mods in development. They are too intense for my tastes. SO I designed my own.

KSP Forum Post Location
GOALS for plugin: 	Track life support use even when ships are not loaded into scene.
     Realistic Life Support values on use and weight. based on information from Life Support Systems at wiki.org
     Low overhead and easy playability with KSP.
     Life support tracked and used on EVA.
     Use only one new resource to simulate LS use.
     Use Electric Charge while Life support is active.
Current Working Features 	Kerbal going on Eva takes Life Support from pod/vessel.
Boarding a pod returns unused Life support to pod/vessel.
Running out of Life Support or Electric can kill crew. if outside kerbin atmosphere.
Life Support and Electric is used even if not active vessel (electric code could be problem on unfocused vessel with solar panels(testing)).
All stock pods have LS Resource and plugin installed using ModuleManager.
Electric Charge on EVA - Used by Lifesupport system and if HeadLamp is on.
To Do List: 	

Test and Debug Code problems.( IN PROGRESS)
Optimize working Code.(ALWAYS WORKING ON)
Create Custom Parts for additional Life support storage.
Create ModuleManager file for third-party command pods.
Add ElectricCharge to EVA(DONE)
Add Laythe to Breathable Atmosphere for reduced consumption? (Checking as Jets work there but might be too thin for Kerbals)
Plugin
Operation
Info 	Currently there are several Status LS system can be in:
           Inactive   - No demand for LS and no resources consumed.  Life Support tag for days / hours of LS remaining will read 0.
            Active       - Demand for LS and resources consumed.  Life Support tag for days / hours of LS remaining will read how long LS will last for whole vessel.
            Visor        - Kerbal on EVA breathing outside air decreased Resource consumption .  Life Support tag for days / hours of LS remaining will read 0 (fixing).
            Intake Air - Pod using air intakes to provide O2 to crew - decreased Resource consumption.
            CAUTION - Less than 2 days pod or 1 hour EVA of LS remaining.  Life Support tag for days / hours of LS remaining will read how long LS will last for whole vessel.
            Warning!  - LS or Electric Charge at 0. Kerbals will start dieing if immediate action not taken. Life Support tag for days / hours of LS remaining will read 0.

    Each unit of LifeSupport should provide 1 Kerbin Day(6 hours) of Life support for 1 Kerbal.
If plugin seems not to be working right you can enable Debugging log entries via the right click menu on any pod (recommended to be left off unless you are experiencing problems as it does spam the log file in this early release. Issues can be report in forum thread or at GitHub.

The Structural Fuselage has LifeSupport resource added to give you a way to carry extra LS in this early release use the tweak-able right click menu in the assembly buildings to set amount defaults to 0 so it won't effect current builds.
