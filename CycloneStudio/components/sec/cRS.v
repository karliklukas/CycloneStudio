module cRS(input wire R, input wire S, input wire CLK, 
output wire Q, output wire Qn);

wire w1,w2; 

nand b1(w1,CLK,S); 
nand b2(w2,CLK,R); 
nand b3(Q,w1,Qn);
nand b4(Qn,w2,Q);

endmodule
