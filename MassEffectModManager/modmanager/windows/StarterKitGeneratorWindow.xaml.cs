﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using IniParser;
using IniParser.Model;
using MassEffectModManager.GameDirectories;
using MassEffectModManager.modmanager.helpers;
using MassEffectModManager.ui;

namespace MassEffectModManager.modmanager.windows
{
    /// <summary>
    /// Interaction logic for StarterKitGeneratorWindow.xaml
    /// </summary>
    public partial class StarterKitGeneratorWindow : Window, INotifyPropertyChanged
    {
        public static GridLength VisibleRowHeight { get; } = new GridLength(25);
        public string BusyText { get; set; }
        public bool IsBusy { get; set; }
        public string ModDescription { get; set; } = "";
        public string ModDeveloper { get; set; } = "";
        public string ModName { get; set; } = "";
        public string ModInternalName { get; set; } = "";
        public string ModDLCFolderName { get; set; } = "";
        public int ModMountPriority { get; set; }
        public int ModInternalTLKID { get; set; }
        public string ModURL { get; set; } = "";
        public EMountFileFlag ModMountFlag { get; set; }
        public ObservableCollectionExtended<UIMountFlag> DisplayedMountFlags { get; } = new ObservableCollectionExtended<UIMountFlag>();
        private readonly List<UIMountFlag> ME1MountFlags = new List<UIMountFlag>();
        private readonly List<UIMountFlag> ME2MountFlags = new List<UIMountFlag>();
        private readonly List<UIMountFlag> ME3MountFlags = new List<UIMountFlag>();
        public string DescriptionWatermarkText
        {
            get { return "Mod Manager description that user will see when they select your mod in Mod Manager"; }
        }
        public StarterKitGeneratorWindow(Mod.MEGame Game)
        {
            ME1MountFlags.Add(new UIMountFlag(EMountFileFlag.ME1_NoSaveFileDependency, "No save file dependency on DLC"));
            ME1MountFlags.Add(new UIMountFlag(EMountFileFlag.ME1_SaveFileDependency, "Save file dependency on DLC"));

            ME2MountFlags.Add(new UIMountFlag(EMountFileFlag.ME2_NoSaveFileDependency, "0x01 | No save file dependency on DLC"));
            ME2MountFlags.Add(new UIMountFlag(EMountFileFlag.ME2_SaveFileDependency, "0x02 | Save file dependency on DLC"));

            ME3MountFlags.Add(new UIMountFlag(EMountFileFlag.ME3_SPOnly_NoSaveFileDependency, "0x08 - SP only | No file dependency on DLC"));
            ME3MountFlags.Add(new UIMountFlag(EMountFileFlag.ME3_SPOnly_SaveFileDependency, "0x09 - SP only | Save file dependency on DLC"));
            ME3MountFlags.Add(new UIMountFlag(EMountFileFlag.ME3_SPMP_SaveFileDependency, "0x1C - SP & MP | No save file dependency on DLC"));
            ME3MountFlags.Add(new UIMountFlag(EMountFileFlag.ME3_MPOnly_Patch, "0x0C - MP only | Loads in MP (PATCH)"));
            ME3MountFlags.Add(new UIMountFlag(EMountFileFlag.ME3_MPOnly_2, "0x14 - MP only | Loads in MP"));
            ME3MountFlags.Add(new UIMountFlag(EMountFileFlag.ME3_MPOnly_2, "0x34 - MP only | Loads in MP"));

            DisplayedMountFlags.Add(new UIMountFlag(EMountFileFlag.ME1_NoSaveFileDependency, "Loading placeholder"));
            DataContext = this;
            LoadCommands();
#if DEBUG
            ModName = "Debug Test Mod";
            ModDeveloper = "Developer";
            ModInternalName = "StarterKit Mod";
            ModDLCFolderName = "StarterKitMod";
            ModMountPriority = 3678;
            ModURL = "https://example.com";
            ModInternalTLKID = 277346578;
            ModDescription = "This is a starter kit debug testing mod.\n\nHerp a derp flerp.";
#endif
            InitializeComponent();
            SetGame(Game);
        }

        public ICommand GenerateStarterKitCommand { get; set; }
        private void LoadCommands()
        {
            GenerateStarterKitCommand = new GenericCommand(RunStarterKitGenerator, ValidateInput);
        }

        private void RunStarterKitGenerator()
        {

            StarterKitOptions sko = new StarterKitOptions
            {
                ModName = ModName,
                ModDescription = ModDescription,
                ModDeveloper = ModDeveloper,
                ModDLCFolderName = ModDLCFolderName,
                ModGame = Game,
                ModInternalName = ModInternalName,
                ModInternalTLKID = ModInternalTLKID,
                ModMountFlag = ModMountFlag,
                ModMountPriority = ModMountPriority,
                ModURL = ModURL
            };
            IsBusy = true;
            BusyText = "Generating mod";
            CreateStarterKitMod(sko, s => { BusyText = s; }, FinishedCallback);
        }

        private void FinishedCallback(Mod obj)
        {
            IsBusy = false;
            if (Owner is MainWindow w)
            {
                w.LoadMods(obj);
            }

            Close();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrEmpty(ModName)) return false;
            if (string.IsNullOrEmpty(ModDLCFolderName)) return false;
            if (ModMountPriority <= 0 || ModMountPriority > 4800) return false; //todo: adjust for each game
            if (ModInternalTLKID < 0) return false;
            if (Game != Mod.MEGame.ME1 && string.IsNullOrEmpty(ModInternalName)) return false;
            return true;
        }

        public Mod.MEGame Game { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender == ME1_RadioButton)
            {
                SetGame(Mod.MEGame.ME1);
            }

            if (sender == ME2_RadioButton)
            {
                SetGame(Mod.MEGame.ME2);
            }

            if (sender == ME3_RadioButton)
            {
                SetGame(Mod.MEGame.ME3);
            }
        }

        private void SetGame(Mod.MEGame game)
        {
            Game = game;
            if (Game == Mod.MEGame.ME1)
            {
                DisplayedMountFlags.ReplaceAll(ME1MountFlags);
                ME1_RadioButton.IsChecked = true;
            }

            if (Game == Mod.MEGame.ME2)
            {
                DisplayedMountFlags.ReplaceAll(ME2MountFlags);
                ME2_RadioButton.IsChecked = true;
            }

            if (Game == Mod.MEGame.ME3)
            {
                DisplayedMountFlags.ReplaceAll(ME3MountFlags);
                ME3_RadioButton.IsChecked = true;
            }
        }

        public class UIMountFlag
        {
            public UIMountFlag(EMountFileFlag flag, string displayString)
            {
                this.Flag = flag;
                this.DisplayString = displayString;
            }

            public EMountFileFlag Flag { get; }
            public string DisplayString { get; }
        }

        public static void CreateStarterKitMod(StarterKitOptions options, Action<string> UITextCallback, Action<Mod> FinishedCallback)
        {
            NamedBackgroundWorker bw = new NamedBackgroundWorker("StarterKitThread");
            bw.DoWork += (sender, args) =>
            {
                var skOption = args.Argument as StarterKitOptions;

                var dlcFolderName = $"DLC_MOD_{skOption.ModDLCFolderName}";
                var modsDirectory = Utilities.GetModDirectoryForGame(skOption.ModGame);
                var modPath = Directory.CreateDirectory(Path.Combine(modsDirectory, skOption.ModName)).FullName;

                //Creating DLC directories
                var contentDirectory = Directory.CreateDirectory(Path.Combine(modPath, dlcFolderName)).FullName;
                var cookedDir = Directory.CreateDirectory(Path.Combine(contentDirectory, skOption.ModGame == Mod.MEGame.ME3 ? "CookedPCConsole" : "CookedPC")).FullName;
                if (skOption.ModGame == Mod.MEGame.ME1)
                {
                    //AutoLoad.ini
                    IniData autoload = new IniData();
                    autoload["Packages"]["GlobalTalkTable1"] = $"{dlcFolderName}.GlobalTlk_tlk";

                    autoload["ME1DLCMOUNT"]["ModName"] = skOption.ModName;
                    autoload["ME1DLCMOUNT"]["ModMount"] = skOption.ModMountPriority.ToString();
                    new FileIniDataParser().WriteFile(Path.Combine(contentDirectory, "AutoLoad.ini"), autoload);

                    //TLK
                    var dialogdir = Directory.CreateDirectory(Path.Combine(cookedDir, "Packages", "Dialog")).FullName;
                    var tlkGlobalFile = Path.Combine(dialogdir, $"{dlcFolderName}_GlobalTlk.upk");
                    Utilities.ExtractInternalFile("MassEffectModManager.modmanager.starterkit.BlankTlkFile.upk", tlkGlobalFile, true);
                }
                else
                {
                    //ME2, ME3
                    MountFile mf = new MountFile();
                    mf.IsME2 = skOption.ModGame == Mod.MEGame.ME2;
                    mf.MountFlag = skOption.ModMountFlag;
                    mf.ME2Only_DLCFolderName = dlcFolderName;
                    mf.ME2Only_DLCHumanName = skOption.ModName;
                    mf.MountPriority = (ushort)skOption.ModMountPriority;
                    mf.TLKID = skOption.ModInternalTLKID;
                    mf.WriteMountFile(Path.Combine(cookedDir, "Mount.dlc"));

                    if (skOption.ModGame == Mod.MEGame.ME3)
                    {
                        //Extract Default.Sfar
                        Utilities.ExtractInternalFile("MassEffectModManager.modmanager.starterkit.Default.sfar", Path.Combine(cookedDir,"Default.sfar"), true);

                    }
                }

                IniData ini = new IniData();
                ini["ModManager"]["cmmver"] = App.HighestSupportedModDesc.ToString();
                ini["ModInfo"]["game"] = skOption.ModGame.ToString();
                ini["ModInfo"]["modname"] = skOption.ModName;
                ini["ModInfo"]["moddev"] = skOption.ModName;
                ini["ModInfo"]["moddesc"] = Utilities.ConvertNewlineToBr(skOption.ModDescription);
                ini["ModInfo"]["modver"] = 1.0.ToString(CultureInfo.InvariantCulture);
                ini["ModInfo"]["modsite"] = skOption.ModURL;


                var modDescPath = Path.Combine(modPath, "moddesc.ini");
                new FileIniDataParser().WriteFile(modDescPath, ini);
                Mod m = new Mod(modDescPath, skOption.ModGame);
                args.Result = m;
                Thread.Sleep(5999);
            };
            bw.RunWorkerCompleted += (a, b) => { FinishedCallback(b.Result as Mod); };
            bw.RunWorkerAsync(options);
        }

        public class StarterKitOptions
        {
            public string ModDescription;
            public string ModDeveloper;
            public string ModName;
            public string ModInternalName;
            public string ModDLCFolderName;
            public int ModMountPriority;
            public int ModInternalTLKID;
            public string ModURL;
            public EMountFileFlag ModMountFlag;
            public Mod.MEGame ModGame;
        }
    }
}