alter table fatura add id_conta_bancaria_movto int 
go
alter table fatura add constraint fk_fatura_conta_bancaria_movto foreign key (id_conta_bancaria_movto) references conta_bancaria_movto (id_conta_bancaria_movto)