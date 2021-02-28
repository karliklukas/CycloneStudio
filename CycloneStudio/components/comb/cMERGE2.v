module cMERGE2(input wire BUS0, input wire BUS1, output wire [1:0] BUS);
assign BUS = {BUS1, BUS0};
endmodule
