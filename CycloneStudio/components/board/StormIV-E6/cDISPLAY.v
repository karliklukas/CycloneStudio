module cDISPLAY(	
	input wire SEG_a, input wire SEG_b, input wire SEG_c, input wire SEG_d, input wire SEG_e, input wire SEG_f,
	input wire SEG_g, input wire SEG_h, input wire SEG_1, input wire SEG_2, input wire SEG_3, input wire SEG_4,
	
	output wire SEGa, output wire SEGb, output wire SEGc, output wire SEGd, output wire SEGe, output wire SEGf,
	output wire SEGg, output wire SEGh, output wire SEG1, output wire SEG2, output wire SEG3,output wire SEG4);
//hidden: SEGa,SEGb,SEGc,SEGd,SEGe,SEGf,SEGg,SEGh,SEG1,SEG2,SEG3,SEG4
//position: 393,348,StormIV

assign SEGa = SEG_a;
assign SEGb = SEG_b;
assign SEGc = SEG_c;
assign SEGd = SEG_d;
assign SEGe = SEG_e;
assign SEGf = SEG_f;
assign SEGg = SEG_g;
assign SEGh = SEG_h;
assign SEG1 = SEG_1;
assign SEG2 = SEG_2;
assign SEG3 = SEG_3;
assign SEG4 = SEG_4;
endmodule
