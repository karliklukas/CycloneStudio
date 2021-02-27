module cSWITCH(
	input wire SWITCH1, input wire SWITCH2, input wire SWITCH3, input wire SWITCH4,
	output wire SW1, output wire SW2, output wire SW3, output wire SW4);
//hidden: SWITCH1,SWITCH2,SWITCH3,SWITCH4
//position: 200,200,stormIV
assign SW1 = SWITCH1;
assign SW2 = SWITCH2;
assign SW3 = SWITCH3;
assign SW4 = SWITCH4;
endmodule
