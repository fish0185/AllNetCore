﻿' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Threading.Tasks
Imports Microsoft.CodeAnalysis
Imports Roslyn.Test.Utilities

Namespace Microsoft.VisualStudio.LanguageServices.UnitTests.CodeModel.CSharp
    Public Class ExternalCodeParameterTests
        Inherits AbstractCodeParameterTests

#Region "FullName tests"

        <ConditionalWpfFact(GetType(x86)), Trait(Traits.Feature, Traits.Features.CodeModel)>
        Public Async Function TestFullName1() As Task
            Dim code =
<Code>
class C
{
    void Foo(string $$s)
    {
    }
}
</Code>

            Await TestFullName(code, "s")
        End Function

        <ConditionalWpfFact(GetType(x86)), Trait(Traits.Feature, Traits.Features.CodeModel)>
        Public Async Function TestFullName2() As Task
            Dim code =
<Code>
class C
{
    void Foo(ref string $$s)
    {
    }
}
</Code>

            Await TestFullName(code, "s")
        End Function

        <ConditionalWpfFact(GetType(x86)), Trait(Traits.Feature, Traits.Features.CodeModel)>
        Public Async Function TestFullName3() As Task
            Dim code =
<Code>
class C
{
    void Foo(out string $$s)
    {
    }
}
</Code>

            Await TestFullName(code, "s")
        End Function

#End Region

#Region "Name tests"

        <ConditionalWpfFact(GetType(x86)), Trait(Traits.Feature, Traits.Features.CodeModel)>
        Public Async Function TestName1() As Task
            Dim code =
<Code>
class C
{
    void Foo(string $$s)
    {
    }
}
</Code>

            Await TestName(code, "s")
        End Function

        <ConditionalWpfFact(GetType(x86)), Trait(Traits.Feature, Traits.Features.CodeModel)>
        Public Async Function TestName2() As Task
            Dim code =
<Code>
class C
{
    void Foo(ref string $$s)
    {
    }
}
</Code>

            Await TestName(code, "s")
        End Function

        <ConditionalWpfFact(GetType(x86)), Trait(Traits.Feature, Traits.Features.CodeModel)>
        Public Async Function TestName3() As Task
            Dim code =
<Code>
class C
{
    void Foo(out string $$s)
    {
    }
}
</Code>

            Await TestName(code, "s")
        End Function

#End Region

        Protected Overrides ReadOnly Property LanguageName As String = LanguageNames.CSharp
        Protected Overrides ReadOnly Property TargetExternalCodeElements As Boolean = True

    End Class
End Namespace