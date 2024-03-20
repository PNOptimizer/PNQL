
This code is for our new Petri-net-based Q-learning scheduling method that efficiently schedules RCM systems. In the method, we use generalized and place-timed Petri nets to model RCM systems, since they can naturally and concisely model system structures, such as conflict, concurrency, and synchronization. Then, we formulate a reinforcement learning method with a sparse Q-table to evaluate state-transition pairs of the net's reachability graph. It uses the negative transition firing time as a reward for an action selection and adopts a large penalty for any encountered deadlock. In addition, it balances the state space exploration and the experience exploitation by using a dynamic $\epsilon$-greedy policy to update the state values with an accumulative reward. Three different dynamic $\epsilon$-greedy policies are designed for different application scenarios. It is a new method to efficiently schedule RCM systems based on Petri nets. Some benchmark RCM systems are tested with the proposed method and some popular PN-based online dispatching rules, such as FIFO and SRPT. Simulation results demonstrate that it schedules RCM systems as quickly as the online dispatching rules while outperforming them in terms of schedule makespan.

They are developed via C#.

If you have any problem with it, please feel free to contact us (huangbo@njust.edu.cn).

March. 13, 2024.
