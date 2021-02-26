module cJK(input wire J, input wire K, input wire CLK, 
output wire Q, output wire Qn);

wire w1,w2; 

nand b1(w1,CLK,J,Qn); 
nand b2(w2,CLK,K,Q); 
nand b3(Q,w1,Qn);
nand b4(Qn,w2,Q);

endmodule
