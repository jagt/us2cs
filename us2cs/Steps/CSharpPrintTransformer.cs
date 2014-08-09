using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.TypeSystem;
using Boo.Lang.Compiler.Steps;

namespace US2CS
{

class CSharpPrintTransformer : AbstractTransformerCompilerStep
{
    private List<string> _importsNames;
    private TypeDefinition _currentDefinition;

    public CSharpPrintTransformer()
    {
        _importsNames = new List<string>();
    }

    public override void Run()
    {
        if (Errors.Count > 0) return;
        Visit(CompileUnit);
    }

    public override void OnModule(Module node)
    {
        Visit(node.Imports);
        Visit(node.Members);
    }

    public override void OnImport(Import node)
    {
        _importsNames.Add(node.Namespace);
    }

    // FIXME doesn't work on all places yet
    public override void OnSimpleTypeReference(SimpleTypeReference node)
    {
        foreach (var spaceName in _importsNames)
        {
            if (node.Name.StartsWith(spaceName) && node.Name.Substring(0, spaceName.Length + 1).Count((c) => (c == '.')) == 0)
            {
                node.Name = node.Name.Substring(0, spaceName.Length + 1);
            }
        }
    }

    public override void OnClassDefinition(ClassDefinition node)
    {
        _currentDefinition = node;
        base.OnClassDefinition(node);
    }

    public override void OnStructDefinition(StructDefinition node)
    {
        _currentDefinition = node;
        base.OnStructDefinition(node);
    }

    public override void OnConstructor(Constructor node)
    {
        node.Name = _currentDefinition.Name;
    }

}

}
