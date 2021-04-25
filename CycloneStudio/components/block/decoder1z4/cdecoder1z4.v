module cdecoder1z4(input wire vstupA,input wire vstupB,output wire vystup1,output wire vystup2,output wire vystup3,output wire vystup4);
//hidden:

wire w9,w10,w11,w12,w1,w2,w3,w4,w5,w6,w7,w8;

cvstupA b1(.vstupA(vstupA),.IN(w9));
cvstupB b2(.vstupB(vstupB),.IN(w10));
cNAND2 b3(.A(w11),.B(w12),.Y(w1));
cNAND2 b4(.A(w12),.B(w9),.Y(w2));
cNAND2 b5(.A(w11),.B(w10),.Y(w3));
cNAND2 b6(.A(w9),.B(w10),.Y(w4));
cNOT b7(.A(w9),.Y(w11));
cNOT b8(.A(w10),.Y(w12));
cNOT b9(.A(w1),.Y(w5));
cNOT b10(.A(w2),.Y(w6));
cNOT b11(.A(w3),.Y(w7));
cNOT b12(.A(w4),.Y(w8));
cvystup1 b13(.OUT(w5),.vystup1(vystup1));
cvystup2 b14(.OUT(w6),.vystup2(vystup2));
cvystup3 b15(.OUT(w7),.vystup3(vystup3));
cvystup4 b16(.OUT(w8),.vystup4(vystup4));

endmodule

