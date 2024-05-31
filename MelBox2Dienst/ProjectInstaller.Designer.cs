﻿namespace MelBox2Dienst
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.serviceProcessInstaller1 = new System.ServiceProcess.ServiceProcessInstaller();
            this.MelBox2ServiceInstaller1 = new System.ServiceProcess.ServiceInstaller();
            // 
            // serviceProcessInstaller1
            // 
            this.serviceProcessInstaller1.Password = null;
            this.serviceProcessInstaller1.Username = null;
            // 
            // MelBox2ServiceInstaller1
            // 
            this.MelBox2ServiceInstaller1.Description = "MelBox2Dienst verarbeitet Störmeldungen";
            this.MelBox2ServiceInstaller1.DisplayName = "MelBox2Service";
            this.MelBox2ServiceInstaller1.ServiceName = "MelBox2Service";
            this.MelBox2ServiceInstaller1.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            this.MelBox2ServiceInstaller1.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.MelBox2ServiceInstaller1_AfterInstall);
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.serviceProcessInstaller1,
            this.MelBox2ServiceInstaller1});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller1;
        private System.ServiceProcess.ServiceInstaller MelBox2ServiceInstaller1;
    }
}