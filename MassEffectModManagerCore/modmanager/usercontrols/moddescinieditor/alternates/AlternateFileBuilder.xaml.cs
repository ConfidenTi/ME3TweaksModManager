﻿using System.ComponentModel;
using System.Linq;
using IniParser.Model;
using MassEffectModManagerCore.modmanager.helpers;
using MassEffectModManagerCore.modmanager.objects;
using MassEffectModManagerCore.modmanager.objects.mod;
using MassEffectModManagerCore.ui;

namespace MassEffectModManagerCore.modmanager.usercontrols.moddescinieditor.alternates
{
    /// <summary>
    /// Interaction logic for AlternateDLCBuilder.xaml
    /// </summary>
    public partial class AlternateFileBuilder : ModdescEditorControlBase, INotifyPropertyChanged
    {
        public ModJob AttachedJob { get; set; }
        /// <summary>
        /// List of editing Alternate DLCs. These have to be extracted out of the job as they are not bindable in job
        /// </summary>
        public ObservableCollectionExtended<AlternateFile> Alternates { get; } = new ObservableCollectionExtended<AlternateFile>();

        public override void OnEditingModChanged(Mod newMod)
        {
            base.OnEditingModChanged(newMod);
            AttachedJob = EditingMod?.GetJob(ModJob.JobHeader.CUSTOMDLC);
            if (AttachedJob != null)
            {
                Alternates.ReplaceAll(AttachedJob.AlternateFiles);
                foreach (var a in Alternates)
                {
                    a.BuildParameterMap();
                }
            }
            else
            {
                Alternates.ClearEx();
            }
        }

        public override void Serialize(IniData ini)
        {
            if (AttachedJob != null && Alternates.Any())
            {
                string outStr = "(";
                bool isFirst = true;
                foreach (var adlc in Alternates)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        outStr += @",";
                    }
                    outStr += StringStructParser.BuildCommaSeparatedSplitValueList(adlc.ParameterMap.Where(x => !string.IsNullOrWhiteSpace(x.Value)).ToDictionary(x => x.Key, x => x.Value));
                }

                outStr += ")";
                ini[@"CUSTOMDLC"][@"altfiles"] = outStr;
            }
        }

        public AlternateFileBuilder()
        {
            LoadCommands();
            InitializeComponent();
        }

        private void LoadCommands()
        {
            AddAlternateFileCommand = new GenericCommand(AddAlternateFile, CanAddAlternateFile);
        }

        private bool CanAddAlternateFile() => EditingMod?.GetJob(ModJob.JobHeader.CUSTOMDLC) != null;

        private void AddAlternateFile()
        {
            Alternates.Add(new AlternateFile($"Alternate File {Alternates.Count + 1}"));
        }


        public GenericCommand AddAlternateFileCommand { get; set; }

        //public ObservableCollectionExtended<AlternateDLC> AlternateDLCs { get; } = new ObservableCollectionExtended<AlternateDLC>();
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
