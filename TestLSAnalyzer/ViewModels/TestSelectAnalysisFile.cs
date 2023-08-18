﻿using LSAnalyzer.Models;
using LSAnalyzer.ViewModels;
using LSAnalyzer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using TestLSAnalyzer.Services;

namespace TestLSAnalyzer.ViewModels
{
    [Collection("Sequential")]
    public class TestSelectAnalysisFile
    {
        [Fact]
        public void TestGuessDatasetType()
        {
            Configuration datasetTypesConfiguration = new(Path.GetTempFileName());
            foreach (var datasetType in DatasetType.CreateDefaultDatasetTypes())
            {
                datasetTypesConfiguration.StoreDatasetType(datasetType);
            }
            Rservice rservice = new();
            Assert.True(rservice.Connect(), "R must also be available for tests");

            SelectAnalysisFile selectAnalysisFileViewModel = new(datasetTypesConfiguration, rservice);
            selectAnalysisFileViewModel.FileName = Path.Combine(TestRservice.AssemblyDirectory, "_testData", "test_nmi10_nrep5.sav");

            bool messageSent = false;
            WeakReferenceMessenger.Default.Register<MultiplePossibleDatasetTypesMessage>(this, (r, m) =>
            {
                messageSent = true;
            });

            selectAnalysisFileViewModel.GuessDatasetTypeCommand.Execute(null);

            Assert.Null(selectAnalysisFileViewModel.SelectedDatasetType);
            Assert.False(messageSent);

            selectAnalysisFileViewModel.DatasetTypes.Add(new()
            {
                Id = 999991,
                Name = "Test with NMI 10 and NREP 5",
                Weight = "wgt",
                NMI = 10,
                MIvar = "mi",
                Nrep = 5,
                RepWgts = "repwgt",
            });

            selectAnalysisFileViewModel.GuessDatasetTypeCommand.Execute(null);

            Assert.NotNull(selectAnalysisFileViewModel.SelectedDatasetType);
            Assert.Equal("Test with NMI 10 and NREP 5", selectAnalysisFileViewModel.SelectedDatasetType.Name);
            Assert.False(messageSent);

            selectAnalysisFileViewModel.DatasetTypes.Add(new()
            {
                Id = 999991,
                Name = "Test with NMI 10 and NREP 5 (duplicate)",
                Weight = "wgt",
                NMI = 10,
                MIvar = "mi",
                Nrep = 5,
                RepWgts = "repwgt",
            });

            selectAnalysisFileViewModel.GuessDatasetTypeCommand.Execute(null);

            Assert.Null(selectAnalysisFileViewModel.SelectedDatasetType);
            Assert.True(messageSent);
        }

        [Fact]
        public void TestUseFileForAnalysisSendsMessageOnFailure()
        {
            Configuration datasetTypesConfiguration = new(Path.GetTempFileName());
            foreach (var datasetType in DatasetType.CreateDefaultDatasetTypes())
            {
                datasetTypesConfiguration.StoreDatasetType(datasetType);
            }
            Rservice rservice = new();
            Assert.True(rservice.Connect(), "R must also be available for tests");

            SelectAnalysisFile selectAnalysisFileViewModel = new(datasetTypesConfiguration, rservice);
            selectAnalysisFileViewModel.FileName = "C:\\dummy.sav";
            selectAnalysisFileViewModel.SelectedDatasetType = selectAnalysisFileViewModel.DatasetTypes.First();

            bool messageSent = false;
            WeakReferenceMessenger.Default.Register<FailureAnalysisConfigurationMessage>(this, (r, m) =>
            {
                messageSent = true;
            });

            selectAnalysisFileViewModel.UseFileForAnalysisCommand.Execute(null);

            Assert.True(messageSent);

            messageSent = false;
            selectAnalysisFileViewModel.FileName = Path.Combine(TestRservice.AssemblyDirectory, "_testData", "test_nmi10_nrep5.sav");

            selectAnalysisFileViewModel.UseFileForAnalysisCommand.Execute(null);

            Assert.True(messageSent);

            messageSent = false;
            selectAnalysisFileViewModel.SelectedDatasetType = new()
            {
                Id = 999991,
                Name = "Test with NMI 10 and NREP 5",
                Weight = "wgtstud",
                NMI = 10,
                MIvar = "mi",
                Nrep = 3,
                RepWgts = "repwgt",
            };

            selectAnalysisFileViewModel.UseFileForAnalysisCommand.Execute(null);

            Assert.True(messageSent);
        }

        [Fact]
        public void TestUseFileForAnalysisSendsMessageOnSuccess()
        {
            Configuration datasetTypesConfiguration = new(Path.GetTempFileName());
            foreach (var datasetType in DatasetType.CreateDefaultDatasetTypes())
            {
                datasetTypesConfiguration.StoreDatasetType(datasetType);
            }
            Rservice rservice = new();
            Assert.True(rservice.Connect(), "R must also be available for tests");

            SelectAnalysisFile selectAnalysisFileViewModel = new(datasetTypesConfiguration, rservice);
            selectAnalysisFileViewModel.FileName = Path.Combine(TestRservice.AssemblyDirectory, "_testData", "test_nmi10_nrep5.sav");
            selectAnalysisFileViewModel.SelectedDatasetType = new()
            {
                Id = 999991,
                Name = "Test with NMI 10 and NREP 5",
                Weight = "wgt",
                NMI = 10,
                MIvar = "mi",
                Nrep = 5,
                RepWgts = "repwgt",
            };

            bool messageSent = false;
            WeakReferenceMessenger.Default.Register<SetAnalysisConfigurationMessage>(this, (r, m) =>
            {
                messageSent = true;
            });

            selectAnalysisFileViewModel.UseFileForAnalysisCommand.Execute(null);

            Assert.True(messageSent);
        }
    }
}
