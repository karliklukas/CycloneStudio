module cMERGE8(input wire BUS0, input wire BUS1, input wire BUS2, input wire BUS3, 
input wire BUS4, input wire BUS5, input wire BUS6, input wire BUS7, output wire [7:0] BUS);
assign BUS = {BUS7, BUS6, BUS5, BUS4, BUS3, BUS2, BUS1, BUS0};
endmodule
