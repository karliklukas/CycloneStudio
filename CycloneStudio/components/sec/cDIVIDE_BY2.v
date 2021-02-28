module cDIVIDE_BY2 ( input wire CLK , input wire RESET, output wire OUT);

always @(posedge CLK)
begin
if (~RESET)
     OUT <= 1'b0;
else
     OUT <= ~OUT;	
end
endmodule
