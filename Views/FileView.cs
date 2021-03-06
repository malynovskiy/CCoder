﻿using CCoder.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CCoder.Views
{
    public class FileView
    {
        public Document Document { get; private set; }
        
        public ICommand NewCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand SaveAsCommand { get; }
        public ICommand OpenCommand { get; }
    
        public FileView(Document document)
        {
            Document = document;

            NewCommand = new RelayCommand(NewFile);
            SaveCommand = new RelayCommand(SaveFile);
            SaveAsCommand = new RelayCommand(SaveFileAs);
            OpenCommand = new RelayCommand(OpenFile);
        }

        public void NewFile()
        {
            Document.FileName = string.Empty;
            Document.FilePath = string.Empty;
            Document.Text = string.Empty;
        }

        private void SaveFile()
        {
            if (Document.FilePath == null || Document.FilePath.Length == 0)
            {
                SaveFileAs();
                return;
            }

            File.WriteAllText(Document.FilePath, Document.Text);
        }
        private void SaveFileAs()
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "C Source File (*.c)|*.c|C++ Source file (*.cpp)|*.cpp|All files (*.*)|*.*";
            if(saveFileDialog.ShowDialog() == true)
            {
                DockFile(saveFileDialog);
                File.WriteAllText(saveFileDialog.FileName, Document.Text);
            }
        }
        private void OpenFile()
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                DockFile(openFileDialog);
                Document.Text = File.ReadAllText(openFileDialog.FileName);
            }
        }

        private void DockFile<T>(T dialog) where T : FileDialog
        {
            Document.FilePath = dialog.FileName;
            Document.FileName = dialog.SafeFileName;
        }

    }
}
