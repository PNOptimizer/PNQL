# PNQL  Petri-net-based Q-learning Scheduling Method

This repository contains the code for our novel **Petri-net-based Q-learning (PNQL)** scheduling method, designed for efficient scheduling of **Robotic Cellular Manufacturing (RCM)** systems. Our approach leverages **generalized and place-timed Petri nets** to model RCM systems effectively, capturing essential system characteristics such as conflict, concurrency, and synchronization.

## Methodology

We propose a reinforcement learning strategy employing a sparse Q-table to assess state-transition pairs within the net's reachability graph. The method utilizes the negative transition firing time as a reward signal for action selection and imposes a substantial penalty for deadlock occurrences. To balance exploration and exploitation, we implement a dynamic $\epsilon$-greedy policy that updates state values with an accumulative reward. Three distinct dynamic $\epsilon$-greedy policies cater to various application scenarios, making PNQL a effective solution for RCM system scheduling.

## Performance

Benchmark tests on standard RCM systems, alongside popular Petri-net-based online dispatching rules like FIFO and SRPT, have shown that PNQL matches the speed of online dispatching while surpassing them in schedule makespan performance.

## Development Environment

The project is developed using **Visual Studio 2022** with **C#** on the **Windows 11** platform.

## Getting Started

To understand and execute this project, please consider the following points:

1. **PNQL.cs**: This file contains the detailed implementation of our PNQL algorithm.
2. **Main.cs**: This file houses the code that manages the algorithm's hyperparameter inputs and handles result outputs.
3. Use the **PNQL.sln** solution file to configure and deploy our C# project.
4. The **\PNQL\bin\Debug\net6.0** directory contains  PN input matrix files and the results of our program.

## Contact

For any inquiries or issues, please reach out to us at [huangbo@njust.edu.cn](mailto:huangbo@njust.edu.cn).

## Version

*March 20, 2024*