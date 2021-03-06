// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (https://www.specflow.org/).
//      SpecFlow Version:3.9.0.0
//      SpecFlow Generator Version:3.9.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace vMotion.Api.Specs.Features
{
    using TechTalk.SpecFlow;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.9.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Xunit.CollectionAttribute("SpecFlowNonParallelizableFeatures")]
    [Xunit.TraitAttribute("Category", "CI")]
    [Xunit.TraitAttribute("Category", "web")]
    [Xunit.TraitAttribute("Category", "mongoDb")]
    [Xunit.TraitAttribute("Category", "nonparallel")]
    public partial class CallAgentQueriesFeature : object, Xunit.IClassFixture<CallAgentQueriesFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private static string[] featureTags = new string[] {
                "CI",
                "web",
                "mongoDb",
                "nonparallel"};
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "CallAgentQueries.feature"
#line hidden
        
        public CallAgentQueriesFeature(CallAgentQueriesFeature.FixtureData fixtureData, vMotion_Api_Specs_XUnitAssemblyFixture assemblyFixture, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Features", "CallAgentQueries", "    As a call agent\r\n    I want to query the call queues", ProgrammingLanguage.CSharp, featureTags);
            testRunner.OnFeatureStart(featureInfo);
        }
        
        public static void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        public void TestInitialize()
        {
        }
        
        public void TestTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public void ScenarioInitialize(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<Xunit.Abstractions.ITestOutputHelper>(_testOutputHelper);
        }
        
        public void ScenarioStart()
        {
            testRunner.OnScenarioStart();
        }
        
        public void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        public virtual void FeatureBackground()
        {
#line 6
#line hidden
#line 7
  testRunner.Given("an operator exists", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 8
  testRunner.Given("a user with role [Agent] using an http web client", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 9
   testRunner.And("purge all calls", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
        }
        
        void System.IDisposable.Dispose()
        {
            this.TestTearDown();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="A call agent queries for calls in-progress")]
        [Xunit.TraitAttribute("FeatureTitle", "CallAgentQueries")]
        [Xunit.TraitAttribute("Description", "A call agent queries for calls in-progress")]
        public void ACallAgentQueriesForCallsIn_Progress()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("A call agent queries for calls in-progress", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 11
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 6
this.FeatureBackground();
#line hidden
                TechTalk.SpecFlow.Table table10 = new TechTalk.SpecFlow.Table(new string[] {
                            "Summary",
                            "OngoingCallBy",
                            "InVideoCallStatus",
                            "AssignedToUserType",
                            "ScheduledIn",
                            "AssignedTo"});
                table10.AddRow(new string[] {
                            "00",
                            "$null$",
                            "",
                            "",
                            "",
                            ""});
                table10.AddRow(new string[] {
                            "01",
                            "$random$",
                            "Active",
                            "Agent",
                            "",
                            ""});
                table10.AddRow(new string[] {
                            "02",
                            "$null$",
                            "",
                            "",
                            "03:00:00:00",
                            ""});
                table10.AddRow(new string[] {
                            "03",
                            "$null$",
                            "",
                            "Agent",
                            "04:00:00:00",
                            "$random$"});
#line 12
  testRunner.Given("with these calls in database", ((string)(null)), table10, "Given ");
#line hidden
#line 19
  testRunner.When("a GET request is sent to [/api/calls/queue?qtype=inprogress]", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 20
  testRunner.Then("the response should be successful", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 21
    testRunner.And("a custom list should have 1 item", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="A call agent queries for calls waiting")]
        [Xunit.TraitAttribute("FeatureTitle", "CallAgentQueries")]
        [Xunit.TraitAttribute("Description", "A call agent queries for calls waiting")]
        public void ACallAgentQueriesForCallsWaiting()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("A call agent queries for calls waiting", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 24
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 6
this.FeatureBackground();
#line hidden
                TechTalk.SpecFlow.Table table11 = new TechTalk.SpecFlow.Table(new string[] {
                            "Summary",
                            "OngoingCallBy",
                            "InVideoCallStatus",
                            "AssignedToUserType",
                            "ScheduledIn",
                            "AssignedTo"});
                table11.AddRow(new string[] {
                            "00",
                            "$null$",
                            "Waiting",
                            "Agent",
                            "",
                            "$null$"});
                table11.AddRow(new string[] {
                            "01",
                            "$random$",
                            "Active",
                            "",
                            "",
                            "$null$"});
                table11.AddRow(new string[] {
                            "02",
                            "$null$",
                            "Waiting",
                            "Agent",
                            "3:00:00:00",
                            "$null$"});
                table11.AddRow(new string[] {
                            "03",
                            "$null$",
                            "Waiting",
                            "Agent",
                            "",
                            "$random$"});
#line 25
  testRunner.Given("with these calls in database", ((string)(null)), table11, "Given ");
#line hidden
#line 32
  testRunner.When("a GET request is sent to [/api/calls/queue?qtype=waiting]", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 33
  testRunner.Then("the response should be successful", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 34
   testRunner.And("a custom list should have 2 items", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="A call agent queries for calls waiting assigned to me", Skip="Ignored")]
        [Xunit.TraitAttribute("FeatureTitle", "CallAgentQueries")]
        [Xunit.TraitAttribute("Description", "A call agent queries for calls waiting assigned to me")]
        public void ACallAgentQueriesForCallsWaitingAssignedToMe()
        {
            string[] tagsOfScenario = new string[] {
                    "ignore"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("A call agent queries for calls waiting assigned to me", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 37
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 6
this.FeatureBackground();
#line hidden
                TechTalk.SpecFlow.Table table12 = new TechTalk.SpecFlow.Table(new string[] {
                            "Summary",
                            "OngoingCallBy",
                            "InVideoCallStatus",
                            "AssignedToUserType",
                            "ScheduledIn",
                            "AssignedTo"});
                table12.AddRow(new string[] {
                            "00",
                            "$null$",
                            "Waiting",
                            "",
                            "",
                            ""});
                table12.AddRow(new string[] {
                            "01",
                            "$random$",
                            "Active",
                            "",
                            "",
                            ""});
                table12.AddRow(new string[] {
                            "02",
                            "$null$",
                            "Waiting",
                            "Agent",
                            "3:00:00:00",
                            ""});
                table12.AddRow(new string[] {
                            "03",
                            "$null$",
                            "Waiting",
                            "Agent",
                            "",
                            "$me$"});
#line 38
  testRunner.Given("with these calls in database", ((string)(null)), table12, "Given ");
#line hidden
#line 45
  testRunner.When("a GET request is sent to [/api/calls/queue?qtype=waiting&assignedTo={staffId}]", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 46
  testRunner.Then("the response should be successful", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 47
   testRunner.And("a custom list should have 1 item", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="A call agent queries for calls scheduled", Skip="Ignored")]
        [Xunit.TraitAttribute("FeatureTitle", "CallAgentQueries")]
        [Xunit.TraitAttribute("Description", "A call agent queries for calls scheduled")]
        public void ACallAgentQueriesForCallsScheduled()
        {
            string[] tagsOfScenario = new string[] {
                    "ignore"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("A call agent queries for calls scheduled", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 50
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 6
this.FeatureBackground();
#line hidden
                TechTalk.SpecFlow.Table table13 = new TechTalk.SpecFlow.Table(new string[] {
                            "Summary",
                            "OngoingCallBy",
                            "InVideoCallStatus",
                            "AssignedToUserType",
                            "ScheduledIn",
                            "AssignedTo"});
                table13.AddRow(new string[] {
                            "00",
                            "",
                            "",
                            "",
                            "",
                            ""});
                table13.AddRow(new string[] {
                            "01",
                            "$random$",
                            "Active",
                            "Agent",
                            "",
                            ""});
                table13.AddRow(new string[] {
                            "02",
                            "",
                            "",
                            "Agent",
                            "3:00:00:00",
                            ""});
                table13.AddRow(new string[] {
                            "03",
                            "",
                            "",
                            "Agent",
                            "",
                            "$random$"});
#line 51
  testRunner.Given("with these calls in database", ((string)(null)), table13, "Given ");
#line hidden
#line 58
  testRunner.When("a GET request is sent to [/api/calls/queue?qtype=scheduled]", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 59
  testRunner.Then("the response should be successful", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 60
   testRunner.And("a custom list should have 1", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="A call agent queries for calls scheduled for me", Skip="Ignored")]
        [Xunit.TraitAttribute("FeatureTitle", "CallAgentQueries")]
        [Xunit.TraitAttribute("Description", "A call agent queries for calls scheduled for me")]
        public void ACallAgentQueriesForCallsScheduledForMe()
        {
            string[] tagsOfScenario = new string[] {
                    "ignore"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("A call agent queries for calls scheduled for me", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 63
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 6
this.FeatureBackground();
#line hidden
                TechTalk.SpecFlow.Table table14 = new TechTalk.SpecFlow.Table(new string[] {
                            "Summary",
                            "OngoingCallBy",
                            "InVideoCallStatus",
                            "AssignedToUserType",
                            "ScheduledIn",
                            "AssignedTo"});
                table14.AddRow(new string[] {
                            "00",
                            "",
                            "",
                            "",
                            "",
                            ""});
                table14.AddRow(new string[] {
                            "01",
                            "$random$",
                            "Active",
                            "",
                            "",
                            ""});
                table14.AddRow(new string[] {
                            "02",
                            "",
                            "",
                            "Agent",
                            "03:00:00:00",
                            "$me$"});
                table14.AddRow(new string[] {
                            "03",
                            "",
                            "",
                            "Agent",
                            "04:00:00:00",
                            "$random$"});
#line 64
  testRunner.Given("with these calls in database", ((string)(null)), table14, "Given ");
#line hidden
#line 71
  testRunner.When("a GET request is sent to [/api/calls/queue?qtype=scheduled]", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 72
  testRunner.Then("the response should be successful", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 73
    testRunner.And("a custom list should have 2", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.9.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : System.IDisposable
        {
            
            public FixtureData()
            {
                CallAgentQueriesFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                CallAgentQueriesFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
