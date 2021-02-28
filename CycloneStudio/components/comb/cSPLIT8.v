module cSPLIT8(input wire [7:0] BUS, output wire BUS0, output wire BUS1, output wire BUS2, output wire BUS3, output wire BUS4, output wire BUS5, output wire BUS6, output wire BUS7);
assign BUS0 = BUS[0];
assign BUS1 = BUS[1];
assign BUS2 = BUS[2];
assign BUS3 = BUS[3];
assign BUS4 = BUS[4];
assign BUS5 = BUS[5];
assign BUS6 = BUS[6];
assign BUS7 = BUS[7];
endmodule
