
 This code deal with the scheduling problem of robotic cellular manufacturing(RCM) systems. It proposes a new Reinforment Learning search algorithm based on place-timed Petri nets (PNs) and Q-learning（QL）. First, we use generalized and place-timed Petri nets to model RCM systems. Then, we formulate a RL method with a sparse Q-table to evaluate state-transition pairs of the net's reachability graph. It uses the negative transition firing time as a reward for an action selection and a large penalty for any encountered deadlock, and it balances the state exploration the experience exploitation using a dynamic $\epsilon$-greedy policy to update the state values with an accumulative reward. Three different dynamic $\epsilon$-greedy policies are designed for different application scenarios.

They are developed via C#.

March. 13, 2024.
