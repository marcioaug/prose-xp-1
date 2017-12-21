using Xunit;

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.ProgramSynthesis.Transformation.Text;
using Microsoft.ProgramSynthesis.Transformation.Text.Semantics;
using Microsoft.ProgramSynthesis.Wrangling.Constraints;


namespace ProseMutant.Tests
{
    public class MutantsWithProseTransform
    {
        [Fact]
        public void TestTextTransformation()
        {

            var examples = new Dictionary<string, string>
            {
                {"Kettil Hansson", "Hansson, K."}
            };

            var program = GetFirstProgram(examples);

            Assert.Equal("Bala, E.", program.Run(ToInput("Etelka Bala")) as string);
            Assert.Equal("Lampros, M.", program.Run(ToInput("Myron, Lampros")) as string);
        }

        [Fact]
        public void TestWithPrograms()
        {
            var examples = new Dictionary<string, string>
            {
                {
@"int function() {
    int a = 0;
    int b = a;
    return b;
}",
@"int function() {
    int a = 0;
    int b = a++;
    return b;
}"
                },
                {
@"int function() {
    int b = 0;
    b = b + 2;
    return b;
}",
@"int function() {
    int b = 0;
    b = b++ + 2;
    return b;
}"
                }
            };

            var program = GetFirstProgram(examples);
            Console.Out.WriteLine(program.ProgramNode.PrintAST(Microsoft.ProgramSynthesis.AST.ASTSerializationFormat.HumanReadable));
            //Console.Out.WriteLine(program.Describe());

            Assert.Equal(
@"int function() {
    int c = 1;
    int d = c++;
    return d;
}",              
                program.Run(ToInput(
@"int function() {
    int c = 1;
    int d = c;
    return d;
}")) as string
            );

            Assert.Equal(
@"int function() {
    int c = 5;
    c = c++ + 8;
    return c;
}",              
                program.Run(ToInput(
@"int function() {
    int c = 5;
    c = c + 8;
    return c;
}")) as string
            );
        }

        public Program GetFirstProgram(Dictionary<string, string> examples)
        {
            return GetPrograms(examples).First();
        }

        public IEnumerable<Program> GetPrograms(Dictionary<string, string> examples)
        {
            return Learner.Instance.LearnTopK(GetConstraints(examples), k: 10);
        }

        public IEnumerable<Constraint<IRow, object>> GetConstraints(Dictionary<string, string>examples)
        {
            
            List<Constraint<IRow, object>> constraints = new List<Constraint<IRow, object>>();
            
            foreach (KeyValuePair<string, string> example in examples)
            {
                constraints.Add(new Example(new InputRow(example.Key), example.Value));
            }

            return constraints;
        }

        public IRow ToInput(string input) {
            return new InputRow(input);
        }
    }
}
