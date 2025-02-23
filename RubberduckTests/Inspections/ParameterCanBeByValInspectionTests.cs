using System.Linq;
using System.Threading;
using NUnit.Framework;
using Rubberduck.Inspections.Concrete;
using Rubberduck.VBEditor.SafeComWrappers;
using RubberduckTests.Mocks;

namespace RubberduckTests.Inspections
{
    [TestFixture]
    public class ParameterCanBeByValInspectionTests
    {
        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_NoResultForByValObjectInInterfaceImplementationProperty()
        {
            const string modelCode = @"
Option Explicit
Public Foo As Long
Public Bar As String
";

            const string interfaceCode = @"
Option Explicit

Public Property Get Model() As MyModel
End Property

Public Property Set Model(ByVal value As MyModel)
End Property

Public Property Get IsCancelled() As Boolean
End Property

Public Sub Show()
End Sub
";

            const string implementationCode = @"
Option Explicit
Private Type TView
    Model As MyModel
    IsCancelled As Boolean
End Type
Private this As TView
Implements IView

Private Property Get IView_IsCancelled() As Boolean
    IView_IsCancelled = this.IsCancelled
End Property

Private Property Set IView_Model(ByVal value As MyModel)
    Set this.Model = value
End Property

Private Property Get IView_Model() As MyModel
    Set IView_Model = this.Model
End Property

Private Sub IView_Show()
    Me.Show vbModal
End Sub
";

            var builder = new MockVbeBuilder();
            var vbe = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("IView", ComponentType.ClassModule, interfaceCode)
                .AddComponent("MyModel", ComponentType.ClassModule, modelCode)
                .AddComponent("MyForm", ComponentType.UserForm, implementationCode)
                .AddProjectToVbeBuilder().Build();

            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.AreEqual(0, inspectionResults.Count());
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_NoResultForByValObjectInProperty()
        {
            const string inputCode =
                @"Public Property Set Foo(ByVal value As Object)
End Property";
            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out _);
            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.AreEqual(0, inspectionResults.Count());
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_NoResultForByValObject()
        {
            const string inputCode =
                @"Sub Foo(ByVal arg1 As Collection)
End Sub";
            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out _);
            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.AreEqual(0, inspectionResults.Count());
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_ReturnsResult_PassedByNotSpecified()
        {
            const string inputCode =
                @"Sub Foo(arg1 As String)
End Sub";
            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out _);
            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.AreEqual(1, inspectionResults.Count());
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_ReturnsResult_PassedByRef_Unassigned()
        {
            const string inputCode =
                @"Sub Foo(ByRef arg1 As String)
End Sub";
            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out _);
            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.AreEqual(1, inspectionResults.Count());
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_ReturnsResult_Multiple()
        {
            const string inputCode =
                @"Sub Foo(arg1 As String, arg2 As Date)
End Sub";
            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out _);
            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.AreEqual(2, inspectionResults.Count());
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_DoesNotReturnResult_PassedByValExplicitly()
        {
            const string inputCode =
                @"Sub Foo(ByVal arg1 As String)
End Sub";
            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out _);
            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.AreEqual(0, inspectionResults.Count());
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_DoesNotReturnResult_PassedByRefAndAssigned()
        {
            const string inputCode =
                @"Sub Foo(ByRef arg1 As String)
    arg1 = ""test""
End Sub";
            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out _);
            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.AreEqual(0, inspectionResults.Count());
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_DoesNotReturnResult_BuiltInEventParam()
        {
            const string inputCode =
                @"Sub Foo(ByRef arg1 As String)
    arg1 = ""test""
End Sub";
            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out _);
            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.AreEqual(0, inspectionResults.Count());
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_ReturnsResult_SomeParams()
        {
            const string inputCode =
                @"Sub Foo(arg1 As String, ByVal arg2 As Integer)
End Sub";
            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out _);
            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.AreEqual(1, inspectionResults.Count());
            }
        }

        [Test]
        [Category("Inspections")]
        public void GivenBtRefArrayParameter_ReturnsNoResult_Method()
        {
            const string inputCode =
                @"Sub Foo(ByRef arg1() As Variant)
End Sub";
            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out _);
            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var results = inspection.GetInspectionResults(CancellationToken.None).ToList();

                Assert.AreEqual(0, results.Count);
            }
        }

        [Test]
        [Category("Inspections")]
        public void GivenBtRefArrayParameter_ReturnsNoResult_Interface()
        {
            //Input
            const string inputCode1 =
                @"Public Sub DoSomething(ByRef a() As Integer)
End Sub";
            const string inputCode2 =
                @"Implements IClass1

Private Sub IClass1_DoSomething(ByRef a() As Integer)
End Sub";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("IClass1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode2)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .Build();
            var vbe = builder.AddProject(project).Build();

            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.IsFalse(inspectionResults.Any());
            }
        }

        [Test]
        [Category("Inspections")]
        public void GivenBtRefArrayParameter_ReturnsNoResult_Event()
        {
            //Input
            const string inputCode1 =
                @"Public Event Foo(ByRef arg1() As Integer)";

            const string inputCode2 =
                @"Private WithEvents abc As Class1

Private Sub abc_Foo(ByRef arg1() As Integer)
End Sub";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .AddComponent("Class3", ComponentType.ClassModule, inputCode2)
                .Build();
            var vbe = builder.AddProject(project).Build();

            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.IsFalse(inspectionResults.Any());
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_DoesNotReturnResult_PassedToByRefProc_NoAssignment()
        {
            const string inputCode =
                @"Sub DoSomething(foo As Integer)
    DoSomethingElse foo
End Sub

Sub DoSomethingElse(ByRef bar As Integer)
End Sub";
            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out _);
            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.IsFalse(inspectionResults.Any(result => result.Target.IdentifierName.Equals("foo")));
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_DoesNotReturnResult_PassedToByRefProc_WithAssignment()
        {
            const string inputCode =
                @"Sub DoSomething(foo As Integer)
    DoSomethingElse foo
End Sub

Sub DoSomethingElse(ByRef bar As Integer)
    bar = 42
End Sub";
            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out _);
            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.IsFalse(inspectionResults.Any());
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_ReturnsResult_PassedToByRefProc_ExplicitlyByVal()
        {
            const string inputCode =
                @"Sub DoSomething(foo As Integer)
    DoSomethingElse (foo)
End Sub

Sub DoSomethingElse(ByRef bar As Integer)
    bar = 42
End Sub";
            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out _);
            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.AreEqual(1, inspectionResults.Count());
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_ReturnsResult_PassedToByRefProc_PartOfExpression()
        {
            const string inputCode =
                @"Sub DoSomething(foo As Integer)
    DoSomethingElse foo + 2
End Sub

Sub DoSomethingElse(ByRef bar As Integer)
    bar = 42
End Sub";
            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out _);
            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.AreEqual(1, inspectionResults.Count());
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_ReturnsResult_PassedToByValEvent()
        {
            const string inputCode =
                @" Public Event Bar(ByVal baz As Integer)

Sub DoSomething(foo As Integer)
    RaiseEvent Bar(foo)
End Sub";
            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out _);
            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.AreEqual(1, inspectionResults.Count());
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_DoesNotReturnResult_PassedToByRefEvent()
        {
            const string inputCode =
                @" Public Event Bar(ByRef baz As Integer)

Sub DoSomething(foo As Integer)
    RaiseEvent Bar(foo)
End Sub";
            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out _);
            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.IsFalse(inspectionResults.Any(result => result.Target.IdentifierName.Equals("foo")));
            }
        }


        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_ReturnsResult_PassedToByRefEvent_ExplicilyByVal()
        {
            const string inputCode =
                @" Public Event Bar(ByRef baz As Integer)

Sub DoSomething(foo As Integer)
    RaiseEvent Bar(BYVAL foo)
End Sub";
            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out _);
            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);
                var fooInspectionResults = inspectionResults.Where(result => result.Target.IdentifierName.Equals("foo"));

                Assert.AreEqual(1, fooInspectionResults.Count());
            }
        }


        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_ReturnsResult_PassedToByRefEvent_PartOfExpression()
        {
            const string inputCode =
                @" Public Event Bar(ByRef baz As Integer)

Sub DoSomething(foo As Integer)
    RaiseEvent Bar(foo + 2)
End Sub";
            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out _);
            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);
                var fooInspectionResults = inspectionResults.Where(result => result.Target.IdentifierName.Equals("foo"));

                Assert.AreEqual(1, fooInspectionResults.Count());
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_Ignored_DoesNotReturnResult()
        {
            const string inputCode =
                @"'@Ignore ParameterCanBeByVal
Sub Foo(arg1 As String)
End Sub";
            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out _);
            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.IsFalse(inspectionResults.Any());
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_InterfaceMember_SingleParam()
        {
            //Input
            const string inputCode1 =
                @"Public Sub DoSomething(ByRef a As Integer)
End Sub";
            const string inputCode2 =
                @"Implements IClass1

Private Sub IClass1_DoSomething(ByRef a As Integer)
End Sub";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("IClass1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode2)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .Build();
            var vbe = builder.AddProject(project).Build();

            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.AreEqual(1, inspectionResults.Count());
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_InterfaceMember_SingleByValParam()
        {
            //Input
            const string inputCode1 =
                @"Public Sub DoSomething(ByVal a As Integer)
End Sub";
            const string inputCode2 =
                @"Implements IClass1

Private Sub IClass1_DoSomething(ByVal a As Integer)
End Sub";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("IClass1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode2)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .Build();
            var vbe = builder.AddProject(project).Build();

            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.IsFalse(inspectionResults.Any());
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_InterfaceMember_SingleParamAssignedTo_InImplementation()
        {
            //Input
            const string inputCode1 =
                @"Public Sub DoSomething(ByRef a As Integer)
End Sub";
            const string inputCode2 =
                @"Implements IClass1

Private Sub IClass1_DoSomething(ByRef a As Integer)
    a = 42
End Sub";
            const string inputCode3 =
                @"Implements IClass1

Private Sub IClass1_DoSomething(ByRef a As Integer)
End Sub";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("IClass1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode2)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode3)
                .Build();
            var vbe = builder.AddProject(project).Build();

            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.IsFalse(inspectionResults.Any());
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_InterfaceMember_SingleParamAssignedTo_InInterface()
        {
            //Input
            const string inputCode1 =
                @"Public Sub DoSomething(ByRef a As Integer)
    a = 42
End Sub";
            const string inputCode2 =
                @"Implements IClass1

Private Sub IClass1_DoSomething(ByRef a As Integer)
End Sub";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("IClass1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode2)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .Build();
            var vbe = builder.AddProject(project).Build();

            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.IsFalse(inspectionResults.Any());
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_InterfaceMember_SingleParamUsedByRefMethod_InImplementation()
        {
            //Input
            const string inputCode1 =
                @"Public Sub DoSomething(ByRef a As Integer)
End Sub";
            const string inputCode2 =
                @"Implements IClass1

Private Sub IClass1_DoSomething(ByRef a As Integer)
    DoSomething a
End Sub

Private Sub DoSomething(ByRef bar As Integer)
End Sub";
            const string inputCode3 =
                @"Implements IClass1

Private Sub IClass1_DoSomething(ByRef a As Integer)
End Sub";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("IClass1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode2)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode3)
                .Build();
            var vbe = builder.AddProject(project).Build();

            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.IsFalse(inspectionResults.Any(result => result.Target.IdentifierName.Equals("a")));
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_InterfaceMember_SingleParamUsedByRefMethod_InInterface()
        {
            //Input
            const string inputCode1 =
                @"Public Sub DoSomething(ByRef a As Integer)
    DoSomethingElse a
End Sub

Private Sub DoSomethingElse(ByRef bar As Integer)
End Sub";
            const string inputCode2 =
                @"Implements IClass1

Private Sub IClass1_DoSomething(ByRef a As Integer)
End Sub";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("IClass1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode2)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .Build();
            var vbe = builder.AddProject(project).Build();

            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.IsFalse(inspectionResults.Any(result => result.Target.IdentifierName.Equals("a")));
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_InterfaceMember_SingleParamUsedByRefMethodExplicitlyByVal_InImplementation()
        {
            //Input
            const string inputCode1 =
                @"Public Sub DoSomething(ByRef a As Integer)
End Sub";
            const string inputCode2 =
                @"Implements IClass1

Private Sub IClass1_DoSomething(ByRef a As Integer)
    DoSomething (a)
End Sub

Private Sub DoSomething(ByRef bar As Integer)
End Sub";
            const string inputCode3 =
                @"Implements IClass1

Private Sub IClass1_DoSomething(ByRef a As Integer)
End Sub";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("IClass1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode2)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode3)
                .Build();
            var vbe = builder.AddProject(project).Build();

            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.AreEqual(1,inspectionResults.Count(result => result.Target.IdentifierName.Equals("a")));
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_InterfaceMember_SingleParamUsedByRefMethodExplicitlyByVal_InInterface()
        {
            //Input
            const string inputCode1 =
                @"Public Sub DoSomething(ByRef a As Integer)
    DoSomethingElse (a)
End Sub

Private Sub DoSomethingElse(ByRef bar As Integer)
End Sub";
            const string inputCode2 =
                @"Implements IClass1

Private Sub IClass1_DoSomething(ByRef a As Integer)
End Sub";
            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("IClass1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode2)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .Build();
            var vbe = builder.AddProject(project).Build();

            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.AreEqual(1, inspectionResults.Count(result => result.Target.IdentifierName.Equals("a")));
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_InterfaceMember_SingleParamUsedByRefMethodPartOfExpression_InImplementation()
        {
            //Input
            const string inputCode1 =
                @"Public Sub DoSomething(ByRef a As Integer)
End Sub";
            const string inputCode2 =
                @"Implements IClass1

Private Sub IClass1_DoSomething(ByRef a As Integer)
    DoSomething a + 42
End Sub

Private Sub DoSomething(ByRef bar As Integer)
End Sub";
            const string inputCode3 =
                @"Implements IClass1

Private Sub IClass1_DoSomething(ByRef a As Integer)
End Sub";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("IClass1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode2)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode3)
                .Build();
            var vbe = builder.AddProject(project).Build();

            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.AreEqual(1, inspectionResults.Count(result => result.Target.IdentifierName.Equals("a")));
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_InterfaceMember_SingleParamUsedByRefMethodPartOfExpression_InInterface()
        {
            //Input
            const string inputCode1 =
                @"Public Sub DoSomething(ByRef a As Integer)
    DoSomethingElse a + 42
End Sub

Private Sub DoSomethingElse(ByRef bar As Integer)
End Sub";
            const string inputCode2 =
                @"Implements IClass1

Private Sub IClass1_DoSomething(ByRef a As Integer)
End Sub";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("IClass1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode2)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .Build();
            var vbe = builder.AddProject(project).Build();

            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.AreEqual(1, inspectionResults.Count(result => result.Target.IdentifierName.Equals("a")));
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_InterfaceMember_SingleParamByRefEvent_InImplementation()
        {
            //Input
            const string inputCode1 =
                @"Public Sub DoSomething(ByRef a As Integer)
End Sub";
            const string inputCode2 =
                @"Implements IClass1

Public Event Bar(ByRef foo As Integer)

Private Sub IClass1_DoSomething(ByRef a As Integer)
    RaiseEvent Bar(a)
End Sub";
            const string inputCode3 =
                @"Implements IClass1

Private Sub IClass1_DoSomething(ByRef a As Integer)
End Sub";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("IClass1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode2)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode3)
                .Build();
            var vbe = builder.AddProject(project).Build();

            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.IsFalse(inspectionResults.Any(result => result.Target.IdentifierName.Equals("a")));
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_InterfaceMember_SingleParamByRefEvent_InInterface()
        {
            //Input
            const string inputCode1 =
                @"Public Event Bar(ByRef foo As Integer)

Public Sub DoSomething(ByRef a As Integer)
    RaiseEvent Bar(a)
End Sub";
            const string inputCode2 =
                @"Implements IClass1

Private Sub IClass1_DoSomething(ByRef a As Integer)
End Sub";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("IClass1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode2)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .Build();
            var vbe = builder.AddProject(project).Build();

            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.IsFalse(inspectionResults.Any(result => result.Target.IdentifierName.Equals("a")));
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_InterfaceMember_SingleParamByRefEventExplicitlyByVal_InImplementation()
        {
            //Input
            const string inputCode1 =
                @"Public Sub DoSomething(ByRef a As Integer)
End Sub";
            const string inputCode2 =
                @"Implements IClass1

Public Event Bar(ByRef foo As Integer)

Private Sub IClass1_DoSomething(ByRef a As Integer)
    RaiseEvent Bar(ByVal a)
End Sub";
            const string inputCode3 =
                @"Implements IClass1

Private Sub IClass1_DoSomething(ByRef a As Integer)
End Sub";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("IClass1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode2)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode3)
                .Build();
            var vbe = builder.AddProject(project).Build();

            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.AreEqual(1, inspectionResults.Count(result => result.Target.IdentifierName.Equals("a")));
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_InterfaceMember_SingleParamByRefEventExplicitlyByVal_InInterface()
        {
            //Input
            const string inputCode1 =
                @"Public Event Bar(ByRef foo As Integer)

Public Sub DoSomething(ByRef a As Integer)
    RaiseEvent Bar(ByVal a)
End Sub";
            const string inputCode2 =
                @"Implements IClass1

Private Sub IClass1_DoSomething(ByRef a As Integer)
End Sub";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("IClass1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode2)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .Build();
            var vbe = builder.AddProject(project).Build();

            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.AreEqual(1, inspectionResults.Count(result => result.Target.IdentifierName.Equals("a")));
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_InterfaceMember_SingleParamByRefEventPartOfExpression_InImplementation()
        {
            //Input
            const string inputCode1 =
                @"Public Sub DoSomething(ByRef a As Integer)
End Sub";
            const string inputCode2 =
                @"Implements IClass1

Public Event Bar(ByRef foo As Integer)

Private Sub IClass1_DoSomething(ByRef a As Integer)
    RaiseEvent Bar(a + 15)
End Sub";
            const string inputCode3 =
                @"Implements IClass1

Private Sub IClass1_DoSomething(ByRef a As Integer)
End Sub";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("IClass1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode2)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode3)
                .Build();
            var vbe = builder.AddProject(project).Build();

            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.AreEqual(1, inspectionResults.Count(result => result.Target.IdentifierName.Equals("a")));
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_InterfaceMember_SingleParamByRefEventPartOfExpression_InInterface()
        {
            //Input
            const string inputCode1 =
                @"Public Event Bar(ByRef foo As Integer)

Public Sub DoSomething(ByRef a As Integer)
    RaiseEvent Bar(a + 15)
End Sub";
            const string inputCode2 =
                @"Implements IClass1

Private Sub IClass1_DoSomething(ByRef a As Integer)
End Sub";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("IClass1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode2)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .Build();
            var vbe = builder.AddProject(project).Build();

            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.AreEqual(1, inspectionResults.Count(result => result.Target.IdentifierName.Equals("a")));
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_InterfaceMember_MultipleParams_OneCanBeByVal_InImplementation()
        {
            //Input
            const string inputCode1 =
                @"Public Sub DoSomething(ByRef a As Integer, ByRef b As Integer)
End Sub";
            const string inputCode2 =
                @"Implements IClass1

Private Sub IClass1_DoSomething(ByRef a As Integer, ByRef b As Integer)
    b = 42
End Sub";
            const string inputCode3 =
                @"Implements IClass1

Private Sub IClass1_DoSomething(ByRef a As Integer, ByRef b As Integer)
End Sub";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("IClass1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode2)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode3)
                .Build();
            var vbe = builder.AddProject(project).Build();

            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.AreEqual("a", inspectionResults.Single().Target.IdentifierName);
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_InterfaceMember_MultipleParams_OneCanBeByVal_InInterface()
        {
            //Input
            const string inputCode1 =
                @"Public Sub DoSomething(ByRef a As Integer, ByRef b As Integer)
    b = 42
End Sub";
            const string inputCode2 =
                @"Implements IClass1

Private Sub IClass1_DoSomething(ByRef a As Integer, ByRef b As Integer)
End Sub";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("IClass1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode2)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .Build();
            var vbe = builder.AddProject(project).Build();

            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.AreEqual("a", inspectionResults.Single().Target.IdentifierName);
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_EventMember_SingleParam()
        {
            //Input
            const string inputCode1 =
                @"Public Event Foo(ByRef arg1 As Integer)";

            const string inputCode2 =
                @"Private WithEvents abc As Class1

Private Sub abc_Foo(ByRef arg1 As Integer)
End Sub";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .AddComponent("Class3", ComponentType.ClassModule, inputCode2)
                .Build();
            var vbe = builder.AddProject(project).Build();

            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.AreEqual(1, inspectionResults.Count());
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_EventMember_SingleByValParam()
        {
            //Input
            const string inputCode1 =
                @"Public Event Foo(ByVal arg1 As Integer)";

            const string inputCode2 =
                @"Private WithEvents abc As Class1

Private Sub abc_Foo(ByVal arg1 As Integer)
End Sub";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .AddComponent("Class3", ComponentType.ClassModule, inputCode2)
                .Build();
            var vbe = builder.AddProject(project).Build();

            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.IsFalse(inspectionResults.Any());
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_EventMember_SingleParamAssignedTo()
        {
            //Input
            const string inputCode1 =
                @"Public Event Foo(ByRef arg1 As Integer)";

            const string inputCode2 =
                @"Private WithEvents abc As Class1

Private Sub abc_Foo(ByRef arg1 As Integer)
    arg1 = 42
End Sub";

            const string inputCode3 =
                @"Private WithEvents abc As Class1

Private Sub abc_Foo(ByRef arg1 As Integer)
End Sub";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .AddComponent("Class3", ComponentType.ClassModule, inputCode3)
                .Build();
            var vbe = builder.AddProject(project).Build();

            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.IsFalse(inspectionResults.Any());
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_EventMember_SingleParamPassedToByRefProcedure()
        {
            //Input
            const string inputCode1 =
                @"Public Event Foo(ByRef arg1 As Integer)";

            const string inputCode2 =
                @"Private WithEvents abc As Class1

Private Sub abc_Foo(ByRef arg1 As Integer)
    DoSomething arg1
End Sub

Private Sub DoSomething(ByRef bar As Integer)
End Sub
";

            const string inputCode3 =
                @"Private WithEvents abc As Class1

Private Sub abc_Foo(ByRef arg1 As Integer)
End Sub";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .AddComponent("Class3", ComponentType.ClassModule, inputCode3)
                .Build();
            var vbe = builder.AddProject(project).Build();

            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.IsFalse(inspectionResults.Any(result => result.Target.IdentifierName.Equals("arg1")));
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_EventMember_SingleParamPassedToByRefProcedureExplicitlyByVal()
        {
            //Input
            const string inputCode1 =
                @"Public Event Foo(ByRef arg1 As Integer)";

            const string inputCode2 =
                @"Private WithEvents abc As Class1

Private Sub abc_Foo(ByRef arg1 As Integer)
    DoSomething (arg1)
End Sub

Private Sub DoSomething(ByRef bar As Integer)
End Sub
";

            const string inputCode3 =
                @"Private WithEvents abc As Class1

Private Sub abc_Foo(ByRef arg1 As Integer)
End Sub";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .AddComponent("Class3", ComponentType.ClassModule, inputCode3)
                .Build();
            var vbe = builder.AddProject(project).Build();

            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.AreEqual(1, inspectionResults.Count(result => result.Target.IdentifierName.Equals("arg1")));
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_EventMember_SingleParamPassedToByRefProcedurePartOfExpression()
        {
            //Input
            const string inputCode1 =
                @"Public Event Foo(ByRef arg1 As Integer)";

            const string inputCode2 =
                @"Private WithEvents abc As Class1

Private Sub abc_Foo(ByRef arg1 As Integer)
    DoSomething arg1 + 2
End Sub

Private Sub DoSomething(ByRef bar As Integer)
End Sub
";

            const string inputCode3 =
                @"Private WithEvents abc As Class1

Private Sub abc_Foo(ByRef arg1 As Integer)
End Sub";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .AddComponent("Class3", ComponentType.ClassModule, inputCode3)
                .Build();
            var vbe = builder.AddProject(project).Build();

            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.AreEqual(1, inspectionResults.Count(result => result.Target.IdentifierName.Equals("arg1")));
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_EventMember_SingleParamPassedToByRefRaiseEvent()
        {
            //Input
            const string inputCode1 =
                @"Public Event Foo(ByRef arg1 As Integer)";

            const string inputCode2 =
                @"Private WithEvents abc As Class1

Public Event Bar(ByRef baz As Integer)

Private Sub abc_Foo(ByRef arg1 As Integer)
    RaiseEvent Bar(arg1)
End Sub
";

            const string inputCode3 =
                @"Private WithEvents abc As Class1

Private Sub abc_Foo(ByRef arg1 As Integer)
End Sub";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .AddComponent("Class3", ComponentType.ClassModule, inputCode3)
                .Build();
            var vbe = builder.AddProject(project).Build();

            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.IsFalse(inspectionResults.Any(result => result.Target.IdentifierName.Equals("arg1")));
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_EventMember_SingleParamPassedToByRefRaiseEventExplicitlyByVal()
        {
            //Input
            const string inputCode1 =
                @"Public Event Foo(ByRef arg1 As Integer)";

            const string inputCode2 =
                @"Private WithEvents abc As Class1

Public Event Bar(ByRef baz As Integer)

Private Sub abc_Foo(ByRef arg1 As Integer)
    RaiseEvent Bar(ByVal arg1)
End Sub
";

            const string inputCode3 =
                @"Private WithEvents abc As Class1

Private Sub abc_Foo(ByRef arg1 As Integer)
End Sub";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .AddComponent("Class3", ComponentType.ClassModule, inputCode3)
                .Build();
            var vbe = builder.AddProject(project).Build();

            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.AreEqual(1, inspectionResults.Count(result => result.Target.IdentifierName.Equals("arg1")));
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_EventMember_SingleParamPassedToByRefRaiseEventPartOfExpression()
        {
            //Input
            const string inputCode1 =
                @"Public Event Foo(ByRef arg1 As Integer)";

            const string inputCode2 =
                @"Private WithEvents abc As Class1

Public Event Bar(ByRef baz As Integer)

Private Sub abc_Foo(ByRef arg1 As Integer)
    RaiseEvent Bar(arg1 + 3)
End Sub
";

            const string inputCode3 =
                @"Private WithEvents abc As Class1

Private Sub abc_Foo(ByRef arg1 As Integer)
End Sub";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .AddComponent("Class3", ComponentType.ClassModule, inputCode3)
                .Build();
            var vbe = builder.AddProject(project).Build();

            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.AreEqual(1, inspectionResults.Count(result => result.Target.IdentifierName.Equals("arg1")));
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_EventMember_MultipleParams_OneCanBeByVal()
        {
            //Input
            const string inputCode1 =
                @"Public Event Foo(ByRef arg1 As Integer, ByRef arg2 As Integer)";

            const string inputCode2 =
                @"Private WithEvents abc As Class1

Private Sub abc_Foo(ByRef arg1 As Integer, ByRef arg2 As Integer)
    arg1 = 42
End Sub";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .AddComponent("Class3", ComponentType.ClassModule, inputCode2)
                .Build();
            var vbe = builder.AddProject(project).Build();

            using (var state = MockParser.CreateAndParse(vbe.Object))
            {

                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.AreEqual("arg2", inspectionResults.Single().Target.IdentifierName);
            }
        }

        [Test]
        [Category("Inspections")]
        public void ParameterCanBeByVal_EnumMemberParameterCanBeByVal()
        {
            //Input
            const string inputCode = @"Option Explicit
Public Enum TestEnum
    Foo
    Bar
End Enum

Private Sub DoSomething(e As TestEnum)
    Debug.Print e
End Sub";

            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out _);
            using (var state = MockParser.CreateAndParse(vbe.Object))
            {
                var inspection = new ParameterCanBeByValInspection(state);
                var inspectionResults = inspection.GetInspectionResults(CancellationToken.None);

                Assert.AreEqual("e", inspectionResults.Single().Target.IdentifierName);
            }
        }
        [Test]
        [Category("Inspections")]
        public void InspectionName()
        {
            const string inspectionName = "ParameterCanBeByValInspection";
            var inspection = new ParameterCanBeByValInspection(null);

            Assert.AreEqual(inspectionName, inspection.Name);
        }
    }
}
