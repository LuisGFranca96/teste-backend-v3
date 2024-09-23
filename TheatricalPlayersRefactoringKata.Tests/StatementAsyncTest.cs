using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Xunit;
using Moq;
using TheatricalPlayersRefactoringKata.Domain.Entities;
using System.Collections.Generic;
using TheatricalPlayersRefactoringKata.Domain.enums;

namespace TheatricalPlayersRefactoringKata.Tests
{
    public class StatementProcessorTests
    {
        private  Dictionary<string,Play> _plays = new Dictionary<string, Play>();
        private Invoice _invoice = null;
        private Invoice _invoice1 = null;

        private void CreateInput()
        {
            _plays.Add("hamlet", new Play("Hamlet", 4024, PlayType.tragedy));
            _plays.Add("as-like", new Play("As You Like It", 2670, PlayType.comedy));
            _plays.Add("othello", new Play("Othello", 3560, PlayType.tragedy));

             _invoice = new Invoice(
                "BigCo",
                new List<Performance>
                {
                    new Performance("hamlet", 55),
                    new Performance("as-like", 35),
                    new Performance("othello", 40),
                }
            );


            Invoice _invoice1 = new Invoice(
                "Adrian",
                new List<Performance>
                {
                    new Performance("hamlet", 44),
                    new Performance("othello", 33),
                    new Performance("othello", 22),
                }
            );
        }
        
        [Fact]
        public async Task ProcessStatementsAsync_ProcessesStatementsFromQueue()
        {
            this.CreateInput(); 
            // Arrange
            var mockStatementGenerator = new Mock<IStatementGenerator>(); 
            mockStatementGenerator.Setup(x => x.GenerateStatement())
                .Returns(new Statement { Customer = "" });

            var statementProcessor = new StatementProcessor(mockStatementGenerator.Object);

            var statementInputs = new List<StatementInput>
            {
                new StatementInput(_invoice, _plays),
                new StatementInput(_invoice1, _plays)
            };

            // Act
            foreach (var statementInput in statementInputs)
            {
                statementProcessor.EnqueueStatement(statementInput);
            }

            var cts = new System.Threading.CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(2)); // Adjust timeout as needed

            var processTask = statementProcessor.ProcessStatementsAsync();
            try
            {
                await Task.WhenAny(processTask, Task.Delay(-1, cts.Token));
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Cancelado o processamento");

            }

            // Assert
            mockStatementGenerator.Verify(
                generator => generator.GenerateStatement(),
                Times.AtLeast(statementInputs.Count) 
            );
        }
    }
}