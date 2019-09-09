# Modern patterns of concurrent and parallel programming in .NET 


## Learn how to write correct and deterministic parallel programs 

Become the master of the multicore domain. Learn how to harness the powers of parallel computation and multicore computation to dominate peer applications in finance software, video games, web applications and market analysis. To yield the most performance, computer programmers have to partition and divide computations to maximize the performance while taking full advantage of multicore processors. Start your path from Padawan to Jedi, after this workshop you will be ready to return to work and have code bend to your will. 
Concurrency, multi-core parallelism, and distributed parallelism are powerful and accessible. These computing concepts promise big impacts on the full spectrum of software applications, including video games, web applications and market analytics suites. Programmers of these products know how to partition computations over large distributed datasets in order to take full advantage of modern hardware and computing environments. These programming skills are not easy to master. This course will introduce technologies and tools available to developers at every level who are interested in achieving high-performance, reliable and maintainable code through functional-style parallelism and concurrency.
We are facing a new era of geographically-distributed, highly-available systems with processor designs that are scaling out in core counts quicker than they are scaling up in core speeds. In keeping up with performance expectations as complexities and dataset grow, scaling out software architectures over these modern computing resources is one of the biggest challenges facing developers today. 
This course will introduce you to technologies and tools available to developers at every level who are interested in achieving exceptional performance in applications. You will gain an insight into the best practices necessary to build concurrent and scalable programs in .NET using the functional paradigm, which covers OnPrem and Cloud based applications.

## Requirements

.NET Core Sdk 2.0

Mono (on unix/mac)

Visual Studio Code

C# extension

Ionide-fsharp extension

.NET Core Sdk 2.0

Install the Sdk (not the Runtime):
https://www.microsoft.com/net/download/core#/sdk

In the page there are also the Step-by-step instructions

## Prerequisites:

osx

windows

Check if is installed correctly with dotnet --info should print:

## .NET Command Line Tools (2.0.0)

Product Information:

Version:            2.0.0

## Mono on unix/mac

For unix/mac, Windows doesnt need it
http://www.mono-project.com/download/

Recommended 5.2 , required >= 4.8

the package is the mono-complete, who contains the mono-devel

Check if is installed correctly with mono --version should print:

Mono JIT compiler version 5.2.0.224 (tarball Mon Sep 18 17:33:20 UTC 2017)

## .NET Command Line Tools (2.0.0)

Product Information:
 Version:            2.0.0
 Commit SHA-1 hash:  cdcd1928c9

Runtime Environment:
 OS Name:     debian
 OS Version:  9
 OS Platform: Linux
 RID:         linux-x64
 Base Path:   /usr/share/dotnet/sdk/2.0.0/

## Visual Studio Code

Install:
https://code.visualstudio.com/download

Extensions to install (docs[https://code.visualstudio.com/docs/editor/extension-gallery]):

C# : https://marketplace.visualstudio.com/items?itemName=ms-vscode.csharp 

Ionide-fsharp : https://marketplace.visualstudio.com/items?itemName=Ionide.Ionide-fsharp 


----

# Course Topic 
## Modules

The course is split into 5 modules, each of them contains a presentation (theory) and one exercise (practice).

### Module 1
- Concurrency and Functional Programming to structure your code
- How to write concurrent programming
- The benefits of immutability and isolation
- Why and how to parallelize the code, strategies to write parallel code
- Useful parallel patterns

### Module 2
- Task and Data Parallelism
- Dynamic parallelism
- The TPL in Action
- What is and to avoid False Memory Sharing
- The Fork/Join pattern
- Parallel Reducer and Map-Reduce
- Data Parallelism in Functional Programming - PLINQ
- Partition correctly to maximize performance
- Composing seamlessly parallel operations with tasks
- Implementing Tasks Combinators
- Asynchronous Memoization
- Multi-Producer/Multi-Consumer pattern
- Parallel Pipeline

### Module 3
- Asynchronous and Functional programming
- The motivation behind Asynchronous programming
- The TAP async/await in action
- The bad side of Asynchronous programming
- Scalability and Asynchronous architecture 
- Asynchronous programming on the server side 
- Asynchronous Parallel lops
- F# Async-Workflow
- Declarative Parallelism 
- Error Handling in asynchronous programming
- Composing asynchronous operations
- Throttle asynchronous operations

### Module 4
- Reactive programming
- Composing events and stream processing
- Reactive Extensions and Functional Programming
- Highly composable async event streams
- Concurrent Rx Scheduler

### Module 5
- The Actor programming model
- Motivation behind
- Message Passing concept 
- The TPL Dataflow
- The TPL Dataflow building blocks
- TPL Dataflow and reactive extensions
- Building an Agent in C#
- The F# Agent (MailboxProcessor)
- Immutability and Isolation 
- Connecting Agents
- Building DAG with Agents
- Concurrent Agent Patterns
- Pipeline processing with Agents 
- Composing and taming Agents



