using TheatricalPlayersRefactoringKata.Domain.Entities;
using System.Collections.Generic;
using System;
 using System.Threading.Tasks;
using System.Collections.Concurrent;
namespace TheatricalPlayersRefactoringKata;

public class StatementProcessor
{
    private readonly IStatementGenerator _statementGenerator; 

    public StatementProcessor(IStatementGenerator statementGenerator)
    {
        _statementGenerator = statementGenerator;
    }
    private readonly ConcurrentQueue<StatementInput> _statementQueue = new ConcurrentQueue<StatementInput>();
 

    public StatementProcessor(StatementGenerator statementGenerator)
    {
        _statementGenerator = statementGenerator;
    }

    public void EnqueueStatement(StatementInput statementInput)
    {
        _statementQueue.Enqueue(statementInput);
    }

    public async Task<Statement> GenerateStatementAsync()
    {
        return await Task.Run(()  => _statementGenerator.GenerateStatement());
    }

    public async Task ProcessStatementsAsync()
    {
        while (true) 
        {
            if (_statementQueue.TryDequeue(out var statementInput))
            {
                var statement = await GenerateStatementAsync();
                Console.WriteLine($"Processed statement for customer: {statement.Customer}");
            }
            else
            {
                await Task.Delay(1000);
            }
        }
    }

    public static async Task Main(Invoice invoice,  Dictionary<string, Play> play)
    {
        var statementGenerator = new StatementGenerator(new StatementInput(invoice,play));
        var statementProcessor = new StatementProcessor(statementGenerator);

        await statementProcessor.ProcessStatementsAsync(); 

        for (int i = 1; i <= 5; i++) // Testar 5
        {
            var statementInput = new StatementInput(invoice,new Dictionary<string, Play>());

            statementProcessor.EnqueueStatement(statementInput);
            Console.WriteLine($"Enqueued statement {i}");
            await Task.Delay(500);
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey(); 
    }
}
 