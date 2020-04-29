﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.UnitTests
{
    public class SymbolFinderTests : ServicesTestBase
    {
        [Fact, WorkItem(4973, "https://github.com/dotnet/roslyn/issues/4973")]
        public async Task ImmediatelyDerivedTypes_CSharp()
        {
            var solution = new AdhocWorkspace().CurrentSolution;

            // create portable assembly with an abstract base class
            solution = AddProjectWithMetadataReferences(solution, "PortableProject", LanguageNames.CSharp, @"
namespace N
{
    public abstract class BaseClass { }
}
", MscorlibRefPortable);

            var portableProject = GetPortableProject(solution);

            // create a normal assembly with a type derived from the portable abstract base
            solution = AddProjectWithMetadataReferences(solution, "NormalProject", LanguageNames.CSharp, @"
using N;
namespace M
{
    public class DerivedClass : BaseClass { }
}
", MscorlibRef, portableProject.Id);

            // get symbols for types
            var portableCompilation = await GetPortableProject(solution).GetCompilationAsync();
            var baseClassSymbol = portableCompilation.GetTypeByMetadataName("N.BaseClass");

            var normalCompilation = await solution.Projects.Single(p => p.Name == "NormalProject").GetCompilationAsync();
            var derivedClassSymbol = normalCompilation.GetTypeByMetadataName("M.DerivedClass");

            // verify that the symbols are different (due to retargeting)
            Assert.NotEqual(baseClassSymbol, derivedClassSymbol.BaseType);

            // verify that the dependent types of `N.BaseClass` correctly resolve to `M.DerivedCLass`
            var derivedFromBase = await SymbolFinder.FindDerivedClassesAsync(baseClassSymbol, solution, transitive: false);
            var derivedDependentType = Assert.Single(derivedFromBase);
            Assert.Equal(derivedClassSymbol, derivedDependentType);
        }

        [Fact, WorkItem(4973, "https://github.com/dotnet/roslyn/issues/4973")]
        public async Task ImmediatelyDerivedInterfaces_CSharp()
        {
            var solution = new AdhocWorkspace().CurrentSolution;

            // create portable assembly with an abstract base class
            solution = AddProjectWithMetadataReferences(solution, "PortableProject", LanguageNames.CSharp, @"
namespace N
{
    public interface BaseInterface { }
}
", MscorlibRefPortable);

            var portableProject = GetPortableProject(solution);

            // create a normal assembly with a type derived from the portable abstract base
            solution = AddProjectWithMetadataReferences(solution, "NormalProject", LanguageNames.CSharp, @"
using N;
namespace M
{
    public interface DerivedInterface : BaseInterface { }
}
", MscorlibRef, portableProject.Id);

            // get symbols for types
            var portableCompilation = await GetPortableProject(solution).GetCompilationAsync();
            var baseClassSymbol = portableCompilation.GetTypeByMetadataName("N.BaseInterface");

            var normalCompilation = await solution.Projects.Single(p => p.Name == "NormalProject").GetCompilationAsync();
            var derivedClassSymbol = normalCompilation.GetTypeByMetadataName("M.DerivedInterface");

            // verify that the symbols are different (due to retargeting)
            Assert.NotEqual(baseClassSymbol, derivedClassSymbol.Interfaces[0]);

            // verify that the dependent types of `N.BaseClass` correctly resolve to `M.DerivedCLass`
            var derivedFromBase = await SymbolFinder.FindDerivedInterfacesAsync(baseClassSymbol, solution, transitive: false);
            var derivedDependentType = Assert.Single(derivedFromBase);
            Assert.Equal(derivedClassSymbol, derivedDependentType);
        }

        private static Project GetPortableProject(Solution solution)
            => solution.Projects.Single(p => p.Name == "PortableProject");

        private static Project GetNormalProject(Solution solution)
            => solution.Projects.Single(p => p.Name == "NormalProject");

        [Fact]
        public async Task ImmediatelyDerivedTypes_CSharp_AliasedNames()
        {
            var solution = new AdhocWorkspace().CurrentSolution;

            // create portable assembly with an abstract base class
            solution = AddProjectWithMetadataReferences(solution, "PortableProject", LanguageNames.CSharp, @"
namespace N
{
    public abstract class BaseClass { }
}
", MscorlibRefPortable);

            var portableProject = GetPortableProject(solution);

            // create a normal assembly with a type derived from the portable abstract base
            solution = AddProjectWithMetadataReferences(solution, "NormalProject", LanguageNames.CSharp, @"
using N;
using Alias1 = N.BaseClass;

namespace M
{
    using Alias2 = Alias1;

    public class DerivedClass : Alias2 { }
}
", MscorlibRef, portableProject.Id);

            // get symbols for types
            var portableCompilation = await GetPortableProject(solution).GetCompilationAsync();
            var baseClassSymbol = portableCompilation.GetTypeByMetadataName("N.BaseClass");

            var normalCompilation = await solution.Projects.Single(p => p.Name == "NormalProject").GetCompilationAsync();
            var derivedClassSymbol = normalCompilation.GetTypeByMetadataName("M.DerivedClass");

            // verify that the symbols are different (due to retargeting)
            Assert.NotEqual(baseClassSymbol, derivedClassSymbol.BaseType);

            // verify that the dependent types of `N.BaseClass` correctly resolve to `M.DerivedCLass`
            var derivedFromBase = await SymbolFinder.FindDerivedClassesAsync(baseClassSymbol, solution, transitive: false);
            var derivedDependentType = Assert.Single(derivedFromBase);
            Assert.Equal(derivedClassSymbol, derivedDependentType);
        }

        [Fact, WorkItem(4973, "https://github.com/dotnet/roslyn/issues/4973")]
        public async Task ImmediatelyDerivedTypes_CSharp_PortableProfile7()
        {
            var solution = new AdhocWorkspace().CurrentSolution;

            // create portable assembly with an abstract base class
            solution = AddProjectWithMetadataReferences(solution, "PortableProject", LanguageNames.CSharp, @"
namespace N
{
    public abstract class BaseClass { }
}
", MscorlibRefPortable);

            var portableProject = GetPortableProject(solution);

            // create a normal assembly with a type derived from the portable abstract base
            solution = AddProjectWithMetadataReferences(solution, "NormalProject", LanguageNames.CSharp, @"
using N;
namespace M
{
    public class DerivedClass : BaseClass { }
}
", SystemRuntimePP7Ref, portableProject.Id);

            // get symbols for types
            var portableCompilation = await GetPortableProject(solution).GetCompilationAsync();
            var baseClassSymbol = portableCompilation.GetTypeByMetadataName("N.BaseClass");

            var normalCompilation = await solution.Projects.Single(p => p.Name == "NormalProject").GetCompilationAsync();
            var derivedClassSymbol = normalCompilation.GetTypeByMetadataName("M.DerivedClass");

            // verify that the symbols are different (due to retargeting)
            Assert.NotEqual(baseClassSymbol, derivedClassSymbol.BaseType);

            // verify that the dependent types of `N.BaseClass` correctly resolve to `M.DerivedCLass`
            var derivedFromBase = await SymbolFinder.FindDerivedClassesAsync(baseClassSymbol, solution, transitive: false);
            var derivedDependentType = Assert.Single(derivedFromBase);
            Assert.Equal(derivedClassSymbol, derivedDependentType);
        }

        [Fact, WorkItem(4973, "https://github.com/dotnet/roslyn/issues/4973")]
        public async Task ImmediatelyDerivedTypes_VisualBasic()
        {
            var solution = new AdhocWorkspace().CurrentSolution;

            // create portable assembly with an abstract base class
            solution = AddProjectWithMetadataReferences(solution, "PortableProject", LanguageNames.VisualBasic, @"
Namespace N
    Public MustInherit Class BaseClass
    End Class
End Namespace
", MscorlibRefPortable);

            var portableProject = GetPortableProject(solution);

            // create a normal assembly with a type derived from the portable abstract base
            solution = AddProjectWithMetadataReferences(solution, "NormalProject", LanguageNames.VisualBasic, @"
Imports N
Namespace M
    Public Class DerivedClass
        Inherits BaseClass
    End Class
End Namespace
", MscorlibRef, portableProject.Id);

            // get symbols for types
            var portableCompilation = await GetPortableProject(solution).GetCompilationAsync();
            var baseClassSymbol = portableCompilation.GetTypeByMetadataName("N.BaseClass");

            var normalCompilation = await solution.Projects.Single(p => p.Name == "NormalProject").GetCompilationAsync();
            var derivedClassSymbol = normalCompilation.GetTypeByMetadataName("M.DerivedClass");

            // verify that the symbols are different (due to retargeting)
            Assert.NotEqual(baseClassSymbol, derivedClassSymbol.BaseType);

            // verify that the dependent types of `N.BaseClass` correctly resolve to `M.DerivedCLass`
            var derivedFromBase = await SymbolFinder.FindDerivedClassesAsync(baseClassSymbol, solution, transitive: false);
            var derivedDependentType = Assert.Single(derivedFromBase);
            Assert.Equal(derivedClassSymbol, derivedDependentType);
        }

        [Fact, WorkItem(4973, "https://github.com/dotnet/roslyn/issues/4973")]
        public async Task ImmediatelyDerivedTypes_CrossLanguage()
        {
            var solution = new AdhocWorkspace().CurrentSolution;

            // create portable assembly with an abstract base class
            solution = AddProjectWithMetadataReferences(solution, "PortableProject", LanguageNames.CSharp, @"
namespace N
{
    public abstract class BaseClass { }
}
", MscorlibRefPortable);

            var portableProject = GetPortableProject(solution);

            // create a normal assembly with a type derived from the portable abstract base
            solution = AddProjectWithMetadataReferences(solution, "NormalProject", LanguageNames.VisualBasic, @"
Imports N
Namespace M
    Public Class DerivedClass
        Inherits BaseClass
    End Class
End Namespace
", MscorlibRef, portableProject.Id);

            // get symbols for types
            var portableCompilation = await GetPortableProject(solution).GetCompilationAsync();
            var baseClassSymbol = portableCompilation.GetTypeByMetadataName("N.BaseClass");

            var normalCompilation = await solution.Projects.Single(p => p.Name == "NormalProject").GetCompilationAsync();
            var derivedClassSymbol = normalCompilation.GetTypeByMetadataName("M.DerivedClass");

            // verify that the symbols are different (due to retargeting)
            Assert.NotEqual(baseClassSymbol, derivedClassSymbol.BaseType);

            // verify that the dependent types of `N.BaseClass` correctly resolve to `M.DerivedCLass`
            var derivedFromBase = await SymbolFinder.FindDerivedClassesAsync(baseClassSymbol, solution, transitive: false);
            var derivedDependentType = Assert.Single(derivedFromBase);
            Assert.Equal(derivedClassSymbol, derivedDependentType);
        }

        [Fact, WorkItem(4973, "https://github.com/dotnet/roslyn/issues/4973")]
        public async Task ImmediatelyDerivedAndImplementingInterfaces_CSharp()
        {
            var solution = new AdhocWorkspace().CurrentSolution;

            // create portable assembly with an interface
            solution = AddProjectWithMetadataReferences(solution, "PortableProject", LanguageNames.CSharp, @"
namespace N
{
    public interface IBaseInterface { }
}
", MscorlibRefPortable);

            var portableProject = GetPortableProject(solution);

            // create a normal assembly with a type implementing that interface
            solution = AddProjectWithMetadataReferences(solution, "NormalProject", LanguageNames.CSharp, @"
using N;
namespace M
{
    public class ImplementingClass : IBaseInterface { }
}
", MscorlibRef, portableProject.Id);

            // get symbols for types
            var portableCompilation = await GetPortableProject(solution).GetCompilationAsync();
            var baseInterfaceSymbol = portableCompilation.GetTypeByMetadataName("N.IBaseInterface");

            var normalCompilation = await solution.Projects.Single(p => p.Name == "NormalProject").GetCompilationAsync();
            var implementingClassSymbol = normalCompilation.GetTypeByMetadataName("M.ImplementingClass");

            // verify that the symbols are different (due to retargeting)
            Assert.NotEqual(baseInterfaceSymbol, Assert.Single(implementingClassSymbol.Interfaces));

            // verify that the implementing types of `N.IBaseInterface` correctly resolve to `M.ImplementingClass`
            var typesThatImplementInterface = await SymbolFinder.FindImplementationsAsync(baseInterfaceSymbol, solution, transitive: false);
            Assert.Equal(implementingClassSymbol, Assert.Single(typesThatImplementInterface));
        }

        [Fact, WorkItem(4973, "https://github.com/dotnet/roslyn/issues/4973")]
        public async Task ImmediatelyDerivedInterfaces_VisualBasic()
        {
            var solution = new AdhocWorkspace().CurrentSolution;

            // create portable assembly with an interface
            solution = AddProjectWithMetadataReferences(solution, "PortableProject", LanguageNames.VisualBasic, @"
Namespace N
    Public Interface IBaseInterface
    End Interface
End Namespace
", MscorlibRefPortable);

            var portableProject = GetPortableProject(solution);

            // create a normal assembly with a type implementing that interface
            solution = AddProjectWithMetadataReferences(solution, "NormalProject", LanguageNames.VisualBasic, @"
Imports N
Namespace M
    Public Class ImplementingClass
        Implements IBaseInterface
    End Class
End Namespace
", MscorlibRef, portableProject.Id);

            // get symbols for types
            var portableCompilation = await GetPortableProject(solution).GetCompilationAsync();
            var baseInterfaceSymbol = portableCompilation.GetTypeByMetadataName("N.IBaseInterface");

            var normalCompilation = await solution.Projects.Single(p => p.Name == "NormalProject").GetCompilationAsync();
            var implementingClassSymbol = normalCompilation.GetTypeByMetadataName("M.ImplementingClass");

            // verify that the symbols are different (due to retargeting)
            Assert.NotEqual(baseInterfaceSymbol, Assert.Single(implementingClassSymbol.Interfaces));

            // verify that the implementing types of `N.IBaseInterface` correctly resolve to `M.ImplementingClass`
            var typesThatImplementInterface = await SymbolFinder.FindImplementationsAsync(baseInterfaceSymbol, solution, transitive: false);
            Assert.Equal(implementingClassSymbol, Assert.Single(typesThatImplementInterface));
        }

        [Fact, WorkItem(4973, "https://github.com/dotnet/roslyn/issues/4973")]
        public async Task ImmediatelyDerivedAndImplementingInterfaces_CrossLanguage()
        {
            var solution = new AdhocWorkspace().CurrentSolution;

            // create portable assembly with an interface
            solution = AddProjectWithMetadataReferences(solution, "PortableProject", LanguageNames.VisualBasic, @"
Namespace N
    Public Interface IBaseInterface
    End Interface
End Namespace
", MscorlibRefPortable);

            var portableProject = GetPortableProject(solution);

            // create a normal assembly with a type implementing that interface
            solution = AddProjectWithMetadataReferences(solution, "NormalProject", LanguageNames.CSharp, @"
using N;
namespace M
{
    public class ImplementingClass : IBaseInterface { }
}
", MscorlibRef, portableProject.Id);

            // get symbols for types
            var portableCompilation = await GetPortableProject(solution).GetCompilationAsync();
            var baseInterfaceSymbol = portableCompilation.GetTypeByMetadataName("N.IBaseInterface");

            var normalCompilation = await solution.Projects.Single(p => p.Name == "NormalProject").GetCompilationAsync();
            var implementingClassSymbol = normalCompilation.GetTypeByMetadataName("M.ImplementingClass");

            // verify that the symbols are different (due to retargeting)
            Assert.NotEqual(baseInterfaceSymbol, Assert.Single(implementingClassSymbol.Interfaces));

            // verify that the implementing types of `N.IBaseInterface` correctly resolve to `M.ImplementingClass`
            var typesThatImplementInterface = await SymbolFinder.FindImplementationsAsync(baseInterfaceSymbol, solution, transitive: false);
            Assert.Equal(implementingClassSymbol, Assert.Single(typesThatImplementInterface));
        }

        [Fact]
        public async Task DerivedMetadataClasses()
        {
            var solution = new AdhocWorkspace().CurrentSolution;

            // create a normal assembly with a type derived from the portable abstract base
            solution = AddProjectWithMetadataReferences(solution, "NormalProject", LanguageNames.CSharp, @"", MscorlibRef);

            // get symbols for types
            var compilation = await GetNormalProject(solution).GetCompilationAsync();
            var rootType = compilation.GetTypeByMetadataName("System.IO.Stream");

            Assert.NotNull(rootType);

            var immediateDerived = await SymbolFinder.FindDerivedClassesAsync(
                rootType, solution, transitive: false);

            Assert.NotEmpty(immediateDerived);
            Assert.True(immediateDerived.All(d => d.BaseType.Equals(rootType)));

            var transitiveDerived = await SymbolFinder.FindDerivedClassesAsync(
                rootType, solution, transitive: true);

            Assert.NotEmpty(transitiveDerived);
            Assert.True(transitiveDerived.All(d => d.GetBaseTypes().Contains(rootType)), "All results must transitively derive from the type");
            Assert.True(transitiveDerived.Any(d => !Equals(d.BaseType, rootType)), "At least one result must not immediately derive from the type");

            Assert.True(transitiveDerived.Count() > immediateDerived.Count());
        }

        [Fact]
        public async Task DerivedSourceInterfaces()
        {
            var solution = new AdhocWorkspace().CurrentSolution;

            // create a normal assembly with a type derived from the portable abstract base
            solution = AddProjectWithMetadataReferences(solution, "NormalProject", LanguageNames.CSharp, @"
interface IA { }

interface IB1 : IA { }
interface IB2 : IA { }
interface IB3 : IEquatable<IB3>, IA { }

interface IC1 : IB1 { }
interface IC2 : IA, IB2 { }
interface IC3 : IB3 { }

interface ID1 : IC1 { }
", MscorlibRef);

            // get symbols for types
            var compilation = await GetNormalProject(solution).GetCompilationAsync();
            var rootType = compilation.GetTypeByMetadataName("IA");

            Assert.NotNull(rootType);

            var immediateDerived = await SymbolFinder.FindDerivedInterfacesAsync(
                rootType, solution, transitive: false);

            Assert.NotEmpty(immediateDerived);
            Assert.True(immediateDerived.All(d => d.Interfaces.Contains(rootType)));

            var transitiveDerived = await SymbolFinder.FindDerivedInterfacesAsync(
                rootType, solution, transitive: true);

            Assert.NotEmpty(transitiveDerived);
            Assert.True(transitiveDerived.All(d => d.AllInterfaces.Contains(rootType)), "All results must transitively derive from the type");
            Assert.True(transitiveDerived.Any(d => !d.Interfaces.Contains(rootType)), "At least one result must not immediately derive from the type");

            Assert.True(transitiveDerived.Count() > immediateDerived.Count());
        }
    }
}
