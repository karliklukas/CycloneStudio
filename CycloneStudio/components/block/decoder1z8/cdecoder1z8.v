module cdecoder1z8(input wire vstup1,input wire vstup2,input wire vstup3,output wire vystup1,output wire vystup2,output wire vystup3,output wire vystup4,output wire vystup5,output wire vystup6,output wire vystup7,output wire vystup8);
//hidden:

wire w3,w2,w1,w4,w20,w5,w22,w6,w7,w21,w8,w9,w10,w11,w12,w13,w14,w15,w16,w17,w18,w19;

cNAND3 b1(.A(w3),.B(w2),.C(w1),.Y(w4));
cNAND3 b2(.A(w3),.B(w2),.C(w20),.Y(w5));
cNAND3 b3(.A(w3),.B(w22),.C(w1),.Y(w6));
cNAND3 b4(.A(w3),.B(w22),.C(w20),.Y(w7));
cNAND3 b5(.A(w21),.B(w2),.C(w1),.Y(w8));
cNAND3 b6(.A(w2),.B(w21),.C(w20),.Y(w9));
cNAND3 b7(.A(w1),.B(w21),.C(w22),.Y(w10));
cNAND3 b8(.A(w20),.B(w22),.C(w21),.Y(w11));
cNOT b9(.A(w4),.Y(w12));
cNOT b10(.A(w5),.Y(w13));
cNOT b11(.A(w6),.Y(w14));
cNOT b12(.A(w7),.Y(w15));
cNOT b13(.A(w8),.Y(w16));
cNOT b14(.A(w9),.Y(w17));
cNOT b15(.A(w10),.Y(w18));
cNOT b16(.A(w11),.Y(w19));
cNOT b17(.A(w1),.Y(w20));
cNOT b18(.A(w2),.Y(w22));
cNOT b19(.A(w3),.Y(w21));
cvstup1 b20(.vstup1(vstup1),.IN(w1));
cvstup2 b21(.vstup2(vstup2),.IN(w2));
cvstup3 b22(.vstup3(vstup3),.IN(w3));
cvystup1 b23(.OUT(w12),.vystup1(vystup1));
cvystup2 b24(.OUT(w13),.vystup2(vystup2));
cvystup3 b25(.OUT(w14),.vystup3(vystup3));
cvystup4 b26(.OUT(w15),.vystup4(vystup4));
cvystup5 b27(.OUT(w16),.vystup5(vystup5));
cvystup6 b28(.OUT(w17),.vystup6(vystup6));
cvystup7 b29(.OUT(w18),.vystup7(vystup7));
cvystup8 b30(.OUT(w19),.vystup8(vystup8));

endmodule

