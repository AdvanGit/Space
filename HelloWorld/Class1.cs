using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using VRageMath;
using VRage.Game;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Ingame;
using Sandbox.Game.EntityComponents;
using VRage.Game.Components;
using VRage.Collections;
using VRage.Game.ObjectBuilders.Definitions;
//using VRage.ModAPI;
using SpaceEngineers.Game.ModAPI.Ingame;

namespace NewHorizon
{
    public sealed class Program : MyGridProgram
    {

        IMyTextSurfaceProvider cockpit;
        IMyMotorStator RotorMain, RotorL, RotorR, AdRotL, AdRotR;
        IMyShipController RemoteControl;
        List<IMyPistonBase> pistonsList = new List<IMyPistonBase>();
        float pistonExtend, rotateAngle, zeroHeading, curElevation, zeroElevation, curFlatAngle, zeroFlatAngle = 0;
        float sensivity = 0.3f;
        string[] dic;
        Action action = Action.Free;
        int page = 0;
        int[] cursor = { 0, 0 };
        bool key1, key2, key3, isManual, isNavigate, isAutomat, isHoldFlat, isHoldHorizon, refreshDisplay = false;

        public Program()
        {

            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            cockpit = GridTerminalSystem.GetBlockWithName("Cockpit") as IMyTextSurfaceProvider;
            RotorMain = GridTerminalSystem.GetBlockWithName("RotorMain") as IMyMotorStator;
            RotorL = GridTerminalSystem.GetBlockWithName("RotorL") as IMyMotorStator;
            RotorR = GridTerminalSystem.GetBlockWithName("RotorR") as IMyMotorStator;
            AdRotL = GridTerminalSystem.GetBlockWithName("AdRotL") as IMyMotorStator;
            AdRotR = GridTerminalSystem.GetBlockWithName("AdRotR") as IMyMotorStator;
            RemoteControl = GridTerminalSystem.GetBlockWithName("Remote") as IMyShipController;
            GridTerminalSystem.GetBlocksOfType<IMyPistonBase>(pistonsList);
            dic = Dictionary("rus");
            WriteStatusDisplay();
            WriteNavigateDisplay(cursor);

        }

        enum Action : int
        {
            Free,
            Secure,
            Extract1,
            Extract2,
            Retrack1,
            Retrack2,
            Retrack3,
            Manual,
 //           Automat
        }
        

        public void WriteStatusDisplay()
        {
            string display = string.Format($"{dic[25],-5}: {getString(action.ToString())}");

            display += string.Format($"\n\n{dic[1],-25} {getString(isNavigate)}");
            display += string.Format($"\n{dic[0],-25} ");

            if ((action != Action.Free) && (action != Action.Manual)) display += string.Format($"{dic[26]}\n");
            else display += string.Format($"{getString(isManual)}\n");

            if (isNavigate) display += string.Format($"\n{dic[21],-30}");
            if ((action == Action.Manual) && (!isHoldFlat)) display += string.Format($"\n{dic[22],-30}");

            if (isHoldHorizon) display += string.Format($"\n\n{dic[27]}");
            if (isHoldFlat) display += string.Format($"\n{dic[33]}");
            cockpit.GetSurface(3).WriteText(display, false);
        }

        public void WriteNavigateDisplay(int[] arr)
        {
            switch (page)
            {
                case 0:
                    {
                        switch (arr[0])
                        {
                            case 0:
                                {
                                    if (arr[1] != 0) page = (arr[1] == 2) ? page += 1 : page = 2;
                                    refreshDisplay = true;
                                    break;
                                }
                            case 1:
                                {
                                    if (arr[1] != 0) isAutomat = (arr[1] == 2) ? true : false;
                                    break;
                                }
                            case 2:
                                {
                                    if (arr[1] != 0) rotateAngle = (arr[1] == 2) ? rotateAngle += 5 : rotateAngle -= 5;
                                    break;
                                }
                            case 3:
                                {
                                    if (arr[1] != 0)
                                    {
                                        pistonExtend = (arr[1] == 2) ? pistonExtend += 0.5f : pistonExtend -= 0.5f;
                                        SetPiston(pistonExtend);
                                    }
                                    break;
                                }
                            case 4:
                                {
                                    if (arr[1] != 0) isHoldHorizon = (arr[1] == 2) ? true : false;
                                    WriteStatusDisplay();
                                    break;
                                }
                            case 5:
                                {
                                    if (arr[1] != 0) isHoldFlat = (arr[1] == 2) ? true : false;
                                    WriteStatusDisplay();
                                    break;
                                }
                            case 6:
                                {
                                    if (arr[1] != 0)
                                    {
                                        isAutomat = false;
                                        rotateAngle = 0;
                                        pistonExtend = 0;
                                        isHoldHorizon = false;
                                        isHoldFlat = false;
                                    }
                                    break;
                                }
                            case 7:
                                {
                                    arr[0] = 0;
                                    break;
                                }
                            case -1:
                                {
                                    arr[0] = 6;
                                    break;
                                }
                        }

                        if (arr[0] == 0) cockpit.GetSurface(1).WriteText($"            < {dic[2]} >\n", false);
                        else cockpit.GetSurface(1).WriteText($"              {dic[2]}\n", false);

                        if (arr[0] == 1) cockpit.GetSurface(1).WriteText($"\n{dic[3]}:   < {getString(isAutomat)} >", true);
                        else cockpit.GetSurface(1).WriteText($"\n{dic[3]}:    {getString(isAutomat)}", true);

                        if (arr[0] == 2) cockpit.GetSurface(1).WriteText($"\n{dic[4]}:   < {rotateAngle} >", true);
                        else cockpit.GetSurface(1).WriteText($"\n{dic[4]}:    {rotateAngle}", true);

                        if (arr[0] == 3) cockpit.GetSurface(1).WriteText($"\n{dic[5]}:   < {pistonExtend} >", true);
                        else cockpit.GetSurface(1).WriteText($"\n{dic[5]}:    {pistonExtend}", true);

                        if (arr[0] == 4) cockpit.GetSurface(1).WriteText($"\n{dic[6]}:   < {getString(isHoldHorizon)} >", true);
                        else cockpit.GetSurface(1).WriteText($"\n{dic[6]}:    {getString(isHoldHorizon)}", true);

                        if (arr[0] == 5) cockpit.GetSurface(1).WriteText($"\n{dic[7]}:   < {getString(isHoldFlat)} >", true);
                        else cockpit.GetSurface(1).WriteText($"\n{dic[7]}:    {getString(isHoldFlat)}", true);

                        if (arr[0] == 6) cockpit.GetSurface(1).WriteText($"\n{dic[17]}   <  >", true);
                        else cockpit.GetSurface(1).WriteText($"\n{dic[17]}", true);
                        break;
                    }
                case 1:
                    {
                        switch (arr[0])
                        {
                            case 0:
                                {
                                    if (arr[1] != 0) page = (arr[1] == 2) ? page += 1 : page -= 1;
                                    refreshDisplay = true;
                                    break;
                                }
                            case 1:
                                {
                                    if (arr[1] != 0)
                                    {
                                        zeroHeading = RotorMain.Angle * 57.2f;
                                        zeroElevation = curElevation;
                                        zeroFlatAngle = (AdRotR.Angle + RotorR.Angle) * 57.2f;
                                    }
                                    break;
                                }
                            case 2:
                                {
                                    if (arr[1] != 0) zeroHeading = (arr[1] == 2) ? zeroHeading += 1 : zeroHeading -= 1;
                                    break;
                                }
                            case 3:
                                {
                                    if (arr[1] != 0) zeroElevation = (arr[1] == 2) ? zeroElevation += 0.25f : zeroElevation -= 0.25f;
                                    break;
                                }
                            case 4:
                                {
                                    if (arr[1] != 0) zeroFlatAngle = (arr[1] == 2) ? zeroFlatAngle += 2.5f : zeroFlatAngle -= 2.5f;
                                    break;
                                }
                            case 5:
                                {
                                    if (arr[1] != 0)
                                    {
                                        zeroHeading = 0;
                                        zeroElevation = 0;
                                        zeroFlatAngle = 0;
                                    }
                                    break;
                                }
                            case 6:
                                {
                                    arr[0] = 0;
                                    break;
                                }
                            case -1:
                                {
                                    arr[0] = 5;
                                    break;
                                }
                        }

                        if (arr[0] == 0) cockpit.GetSurface(1).WriteText($"             < {dic[8]} >\n", false);  //Привязка
                        else cockpit.GetSurface(1).WriteText($"               {dic[8]}\n", false);

                        if (arr[0] == 1) cockpit.GetSurface(1).WriteText($"\n{dic[9]}:       < {RotorMain.Angle * (float)57.2:0.0}, {curElevation:0.0}, {(AdRotR.Angle + RotorR.Angle) * 57.2:0.0} >", true);   //Привязать текущую позицию
                        else cockpit.GetSurface(1).WriteText($"\n{dic[9]}:         {RotorMain.Angle * (float)57.2:0.0}, {curElevation:0.0}, {(AdRotR.Angle + RotorR.Angle) * 57.2:0.0}", true);

                        if (arr[0] == 2) cockpit.GetSurface(1).WriteText($"\n{dic[11]}:      < {zeroHeading:0.0} >", true);
                        else cockpit.GetSurface(1).WriteText($"\n{dic[11]}:        {zeroHeading:0.0}", true);

                        if (arr[0] == 3) cockpit.GetSurface(1).WriteText($"\n{dic[10]}:      < {zeroElevation:0.0} >", true);
                        else cockpit.GetSurface(1).WriteText($"\n{dic[10]}:        {zeroElevation:0.0}", true);

                        if (arr[0] == 4) cockpit.GetSurface(1).WriteText($"\n{dic[12]}:      < {zeroFlatAngle:0.0} >", true);
                        else cockpit.GetSurface(1).WriteText($"\n{dic[12]}:        {zeroFlatAngle:0.0}", true);

                        if (arr[0] == 5) cockpit.GetSurface(1).WriteText($"\n\n{dic[17]}      < >", true);
                        else cockpit.GetSurface(1).WriteText($"\n\n{dic[17]}", true);

                        break;
                    }
                case 2:
                    {
                        switch (arr[0])
                        {
                            case 0:
                                {
                                    if (arr[1] != 0) page = (arr[1] == 2) ? page = 0 : page -= 1;
                                    refreshDisplay = true;
                                    break;
                                }
                            case 1:
                                {
                                    if (arr[1] != 0)
                                    {
                                        dic = (arr[1] == 2) ? Dictionary("eng") : Dictionary("rus");
                                        WriteStatusDisplay();
                                    }
                                    break;
                                }
                            case 2:
                                {
                                    if (arr[1] != 0) sensivity = (arr[1] == 2) ? sensivity += 0.1f : sensivity -= 0.1f;
                                    break;
                                }
                            case 3:
                                {
                                    if (arr[1] != 0) Runtime.UpdateFrequency = (arr[1] == 2) ? UpdateFrequency.Update1 : UpdateFrequency.Update10;
                                    break;
                                }

                            case 4:
                                {
                                    if (arr[1] != 0)
                                    {
                                        dic = Dictionary("eng");
                                        sensivity = 0.3f;
                                        Runtime.UpdateFrequency = UpdateFrequency.Update10;
                                    }
                                    break;
                                }
                            case 5:
                                {
                                    arr[0] = 0;
                                    break;
                                }
                            case -1:
                                {
                                    arr[0] = 4;
                                    break;
                                }
                        }

                        if (arr[0] == 0) cockpit.GetSurface(1).WriteText($"             < {dic[13]} >\n", false);  //Setting
                        else cockpit.GetSurface(1).WriteText($"               {dic[13]}\n", false);

                        if (arr[0] == 1) cockpit.GetSurface(1).WriteText($"\n{dic[14]}:       < {dic[16]} >", true);
                        else cockpit.GetSurface(1).WriteText($"\n{dic[14]}:         {dic[16]}  ", true);

                        if (arr[0] == 2) cockpit.GetSurface(1).WriteText($"\n{dic[18]}:       < {sensivity.ToString()} >", true);
                        else cockpit.GetSurface(1).WriteText($"\n{dic[18]}:         {sensivity.ToString()}  ", true);

                        if (arr[0] == 3) cockpit.GetSurface(1).WriteText($"\n{dic[15]}:      < {getString(Runtime.UpdateFrequency.ToString())} >", true);
                        else cockpit.GetSurface(1).WriteText($"\n{dic[15]}:        {getString(Runtime.UpdateFrequency.ToString())}", true);

                        if (arr[0] == 4) cockpit.GetSurface(1).WriteText($"\n\n\n{dic[17]}      < >", true);
                        else cockpit.GetSurface(1).WriteText($"\n\n\n{dic[17]}", true);

                        break;
                    }
            }
        }

        public void SetPiston(float targetLenght)
        {
            if (pistonsList[0].CurrentPosition >= targetLenght)
            {
                if (pistonsList[0].Velocity >= 0) foreach (var i in pistonsList) i.Velocity *= (-1);
                foreach (var i in pistonsList)
                    i.MinLimit = targetLenght;
            }
            else
            {
                if (pistonsList[0].Velocity <= 0) foreach (var i in pistonsList) i.Velocity *= (-1);
                foreach (var i in pistonsList)
                    i.MaxLimit = targetLenght;
            }

        }

        static string[] Dictionary(string lang)
        {
            switch (lang)
            {
                case "rus":
                    {
                        string[] dic = {"Ручное управление (E):", "Режим навигации (Q):", "ПРОГРАММА", "Автомат", "Вращение", "Подача", "Удержание горизонта","Удержание плоскости", //0-7
                                "ПРИВЯЗКА", "Привязать", "Тангаж", "Курс", "Плоскость", "НАСТРОЙКИ", "Язык", "Приоритет", "Русский", "Сброс", "Чувствительность",                       //8-18
                                "[ВКЛ]", "  [x]  ", "21","22","Высокий","Средний","СТАТУС","Блок.","27","Ожидание","Заперто","Ручное упр.","Выдвижение...","Задвижение...","33"};                                                                                    //19+
                        dic[21] = "(исп. \"WASD\" для навигации)       ";
                        dic[22] = "(уд. \"C\" для вращения плоскости)";
                        dic[27] = "Удержание горизонта...";
                        dic[33] = "Удержание плоскости...";
                        return dic;
                    }
                case "eng":
                default:
                    {
                        string[] dic = {"Manual Mode (E):", "Navigate Mode (Q):", "PROGRAM", "Automat", "Roll", "Feed", "Hold Horizon", "Hold Flat",     //0-7
                                "BINDING", "Bind", "Pitch", "Heading", "Flat", "SETTING", "Language", "Priority", "English", "Reset", "Sensivity",           //8-18
                                "[ON]", "  [x] ", "21", "22" ,"High","Medium","STATUS","locked","27","Idle","Secure","Manual","Extracting...","Retracking...","33"};                                                          //19+
                        dic[21] = "(use \"WASD\" to navigate)";
                        dic[22] = "(hold \"C\" to flat rotate)            ";
                        dic[27] = "Holding horizon...";
                        dic[33] = "Holding flat...";
                        return dic;
                    }
            }
        }
        public string getString(bool str)
        {
            if (str) return dic[19];
            else return dic[20];
        }
        public string getString(string str)
        {
            switch (str)
            {
                case "Update1": return dic[23];
                case "Update10": return dic[24];
                case "Free": return dic[28];
                case "Secure": return dic[29];
                case "Manual": return dic[30];
                case "Extract1":
                case "Extract2": return dic[31];
                case "Retrack1":
                case "Retrack2":
                case "Retrack3": return dic[32];
                default: return null;
            }
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (refreshDisplay)
            {
                cursor[0] = 0;
                cursor[1] = 0;
                WriteNavigateDisplay(cursor);
                refreshDisplay = false;
            }

            switch (argument)
            {
                case "extract":
                    {
                        RotorMain.UpperLimitDeg = float.MaxValue;
                        RotorMain.LowerLimitDeg = float.MinValue;
                        action = Action.Extract1;
                        isHoldFlat = false;
                        break;
                    }
                case "retrack":
                    {
                        action = Action.Retrack1;
                        isHoldHorizon = false;

                        break;
                    }
                case "stop":
                    {
                        action = Action.Free;
                        break;
                    }

            }

            switch (action)
            {
                case Action.Free:
                    {

                        break;
                    }
                case Action.Manual:
                    {
                        RotorMain.TargetVelocityRPM = (RemoteControl.RotationIndicator.Y) * sensivity;

                        if (RemoteControl.MoveIndicator.Y == -1)
                        {

                            RotorL.TargetVelocityRPM = 0;
                            RotorR.TargetVelocityRPM = 0;
                            AdRotL.TargetVelocityRPM = (RemoteControl.RotationIndicator.X) * sensivity;
                            AdRotR.TargetVelocityRPM = -(RemoteControl.RotationIndicator.X) * sensivity;
                        }
                        else
                        {
                            if (!isHoldHorizon)
                            {
                                RotorL.TargetVelocityRPM = (RemoteControl.RotationIndicator.X) * 0.1f;
                                RotorR.TargetVelocityRPM = -(RemoteControl.RotationIndicator.X) * 0.1f;
                            }
                            if (!isHoldFlat)
                            {
                                AdRotL.TargetVelocityRPM = 0;
                                AdRotR.TargetVelocityRPM = 0;
                            }
                        }
                        break;
                    }
                case Action.Secure:
                    {
                        WriteStatusDisplay();
                        break;
                    }
                case Action.Extract1:
                    {
                        SetPiston(10);
                        action = Action.Extract2;
                        WriteStatusDisplay();
                        break;
                    }
                case Action.Extract2:
                    {
                        if (pistonsList[0].CurrentPosition == 10)
                        {

                            //RotorMain.LowerLimitDeg = float.MinValue;
                            // RotorMain.UpperLimitDeg = float.MaxValue;
                            RotorMain.RotorLock = false;
                            RotorL.RotorLock = false;
                            RotorR.RotorLock = false;
                            action = Action.Free;
                            WriteStatusDisplay();
                        }
                        break;

                    }
                case Action.Retrack1:   //heading rotor direction of rotate
                    {

                        if (RotorMain.Angle * 57.2 > 340)
                        {
                            isManual = false;
                            zeroFlatAngle = curElevation;
                            isHoldFlat = true;
                            SetPiston(10);
                            RotorMain.UpperLimitDeg = 0;
                            RotorMain.TargetVelocityRPM = 0.5f;
                            RotorL.TargetVelocityRPM = -0.3f;
                            RotorR.TargetVelocityRPM = 0.3f;
                            action = Action.Retrack2;
                            WriteStatusDisplay();
                            break;
                        }
                        else action = Action.Free;
                        if (RotorMain.Angle * 57.2 < 20)
                        {
                            isManual = false;
                            zeroFlatAngle = curElevation;
                            isHoldFlat = true;
                            SetPiston(10);
                            RotorL.TargetVelocityRPM = -0.3f;
                            RotorR.TargetVelocityRPM = 0.3f;
                            RotorMain.LowerLimitDeg = 360;
                            RotorMain.TargetVelocityRPM = -0.5f;
                            action = Action.Retrack2;
                            WriteStatusDisplay();
                            break;
                        }
                        else action = Action.Free;
                        break;
                    }
                case Action.Retrack2:     //heading zero point
                    {

                        if ((RotorL.Angle * 57.2 < 0) && (AdRotL.Angle * 57.2 > -0.2f))
                            if ((RotorMain.Angle * 57.2 < 360) && (RotorMain.Angle * 57.2 > 0))
                            {

                                RotorL.TargetVelocityRPM = 0;
                                RotorR.TargetVelocityRPM = 0;
                                RotorMain.TargetVelocityRPM = 0;
                                RotorMain.RotorLock = true;
                                RotorL.RotorLock = true;
                                RotorR.RotorLock = true;
                                RotorMain.LowerLimitDeg = float.MinValue;
                                RotorMain.UpperLimitDeg = float.MaxValue;
                                SetPiston(0);
                                action = Action.Retrack3;
                            }
                        break;
                    }
                case Action.Retrack3:
                    {
                        if (pistonsList[0].CurrentPosition == 0)
                        {
                            action = Action.Secure;
                            isHoldFlat = false;
                        }
                        break;
                    }
            }

            switch ((RemoteControl.RollIndicator).ToString())
            {
                case "0":
                    {
                        if (key1)
                        {
                            key1 = false;
                        }
                        break;
                    }
                case "-1":
                    {
                        if (!key1)
                        {
                            isNavigate = !isNavigate;
                            key1 = !key1;
                            WriteStatusDisplay();

                        }
                        break;
                    }
                case "1":
                    {
                        if (!key1)
                        {
                            if (action == Action.Free)
                            {
                                action = Action.Manual;
                                isManual = true;
                                key1 = !key1;
                            }
                            else if (action == Action.Manual)
                            {
                                action = Action.Free;
                                isManual = false;
                                key1 = !key1;
                            }
                            WriteStatusDisplay();
                        }
                        break;
                    }
            } //Change control mode 

            if (isHoldHorizon)
            {

                Vector3D GravNorm = Vector3D.Normalize((cockpit as IMyShipController).GetNaturalGravity());
                float vecForward = (float)GravNorm.Dot((cockpit as IMyShipController).WorldMatrix.Forward);
                float vecUp = (float)GravNorm.Dot((cockpit as IMyShipController).WorldMatrix.Up);
                curElevation = -(float)Math.Atan2(vecForward, -vecUp) * 57.2f;

                RotorL.TargetVelocityRPM = (sensivity / 2) * (curElevation - zeroElevation);
                RotorR.TargetVelocityRPM = -RotorL.TargetVelocityRPM;
            }

            if (isHoldFlat)
            {
                Vector3D GravNorm = Vector3D.Normalize((cockpit as IMyShipController).GetNaturalGravity());
                float vecForward = (float)GravNorm.Dot((cockpit as IMyShipController).WorldMatrix.Forward);
                float vecUp = (float)GravNorm.Dot((cockpit as IMyShipController).WorldMatrix.Up);
                curFlatAngle = -(float)Math.Atan2(vecForward, -vecUp) * 57.2f;

                AdRotL.TargetVelocityRPM = 0.1f * (AdRotR.Angle * 57.2f + curFlatAngle - zeroFlatAngle);
                AdRotR.TargetVelocityRPM = -AdRotL.TargetVelocityRPM;
            }

            if (isNavigate)
            {
                switch ((RemoteControl.MoveIndicator.Z).ToString())
                {
                    case "0":
                        {
                            if (key2)
                                key2 = false;
                            break;
                        }
                    case "-1": //w key pressed
                        {
                            if (!key2)
                            {
                                cursor[0]--;
                                key2 = true;
                                WriteNavigateDisplay(cursor);
                            }
                            break;
                        }
                    case "1":   //s key pressed
                        {
                            if (!key2)
                            {
                                cursor[0]++;
                                key2 = true;
                                WriteNavigateDisplay(cursor);
                            }
                            break;
                        }
                }
                switch ((RemoteControl.MoveIndicator.X).ToString())
                {
                    case "0":
                        {
                            if (key3)
                                key3 = false;
                            cursor[1] = 0;
                            break;
                        }
                    case "1": //D key pressed
                        {
                            if (!key3)
                            {
                                cursor[1] = 2;
                                if (Runtime.UpdateFrequency == UpdateFrequency.Update1) key3 = true;
                                WriteNavigateDisplay(cursor);
                            }
                            break;
                        }
                    case "-1": //A key pressed
                        {
                            if (!key3)
                            {
                                cursor[1] = 1;
                                if (Runtime.UpdateFrequency == UpdateFrequency.Update1) key3 = true;
                                WriteNavigateDisplay(cursor);
                            }
                            break;

                        }
                }
                RemoteControl.ControlWheels = false;
            }
            else RemoteControl.ControlWheels = true;


            //--------DEBUG---------
            cockpit.GetSurface(0).WriteText(cursor[0].ToString() + cursor[1].ToString(), false);
            cockpit.GetSurface(0).WriteText("\nflat: " + isHoldFlat.ToString(), true);
            cockpit.GetSurface(0).WriteText("\nPitchInputEl: " + curElevation, true);
            cockpit.GetSurface(0).WriteText("\nPitchInputFl: " + curFlatAngle, true);
            cockpit.GetSurface(0).WriteText("\nAction: " + action, true);
            cockpit.GetSurface(0).WriteText("\nMain Rotor Angle:  " + (RotorMain.Angle * 57.2).ToString(), true);
            cockpit.GetSurface(0).WriteText("\nAdRotorAngle:  " + (AdRotR.Angle * 57.2).ToString(), true);
            cockpit.GetSurface(0).WriteText("\n" + action.ToString(), true);


        }


    }
}
