module cMyBlock1(input wire Adsd,output wire TTre);
//hidden:

wire w4,w2,w6,w7,w3,w5;

cAdsd b1(.Adsd(Adsd),.IN(w4));
cTTre b2(.OUT(w2),.TTre(TTre));
cAND2 b3(.A(w6),.B(w7),.Y(w2));
cZERO b4(.Y(w3));
cMERGE2 b5(.BUS0(w4),.BUS1(w3),.BUS[1:0](w5));
cSPLIT2 b6(.BUS[1:0](w5),.BUS0(w6),.BUS1(w7));

endmodule

