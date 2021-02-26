module cD(input wire D, input wire CLK, output wire Q, output wire Qn);

wire w1,w2,w3;      

not  b1(w1,D);
nand b2(w2,CLK,D); 
nand b3(w3,CLK,w1); 
nand b4(Q,w2,Qn);
nand b5(Qn,w3,Q);

endmodule
