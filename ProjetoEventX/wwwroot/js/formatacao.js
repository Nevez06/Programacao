// ================================
// ?? FORMATAÇÃO DE HORA E DINHEIRO
// ================================

document.addEventListener('DOMContentLoaded', function() {
    // Aplicar máscaras automaticamente em campos com classes específicas
    aplicarMascaras();
});

function aplicarMascaras() {
    // FORMATAÇÃO DE HORA (HH:MM)
    const camposHora = document.querySelectorAll('.hora-input, input[type="time"], input[name*="Hora"]');
    camposHora.forEach(campo => {
        aplicarMascaraHora(campo);
    });

    // FORMATAÇÃO DE DINHEIRO (R$ 0.000,00)
    const camposDinheiro = document.querySelectorAll('.dinheiro-input, input[name*="Custo"], input[name*="Preco"], input[name*="Valor"]');
    camposDinheiro.forEach(campo => {
        aplicarMascaraDinheiro(campo);
    });

    // FORMATAÇÃO DE CPF
    const camposCpf = document.querySelectorAll('.cpf-input, input[name*="Cpf"]');
    camposCpf.forEach(campo => {
        aplicarMascaraCpf(campo);
    });

    // FORMATAÇÃO DE CNPJ
    const camposCnpj = document.querySelectorAll('.cnpj-input, input[name*="Cnpj"]');
    camposCnpj.forEach(campo => {
        aplicarMascaraCnpj(campo);
    });

    // FORMATAÇÃO DE TELEFONE
    const camposTelefone = document.querySelectorAll('.telefone-input, input[name*="Telefone"]');
    camposTelefone.forEach(campo => {
        aplicarMascaraTelefone(campo);
    });
}

// ================================
// ?? FORMATAÇÃO DE HORA
// ================================
function aplicarMascaraHora(campo) {
    campo.addEventListener('input', function(e) {
        let valor = e.target.value.replace(/\D/g, ''); // Remove tudo que não é número
        
        if (valor.length >= 2) {
            valor = valor.substring(0, 2) + ':' + valor.substring(2, 4);
        }
        
        e.target.value = valor;
    });

    campo.addEventListener('blur', function(e) {
        let valor = e.target.value.replace(/\D/g, '');
        
        if (valor.length === 3 || valor.length === 4) {
            let horas = valor.substring(0, 2);
            let minutos = valor.substring(2, 4);
            
            // Validar horas (00-23)
            if (parseInt(horas) > 23) horas = '23';
            // Validar minutos (00-59)
            if (parseInt(minutos) > 59) minutos = '59';
            
            e.target.value = horas + ':' + (minutos || '00');
        }
    });
}

// ================================
// ?? FORMATAÇÃO DE DINHEIRO
// ================================
function aplicarMascaraDinheiro(campo) {
    // Aplicar formatação inicial se já houver valor
    if (campo.value) {
        campo.value = formatarDinheiro(campo.value);
    }

    campo.addEventListener('input', function(e) {
        e.target.value = formatarDinheiro(e.target.value);
    });
}

function formatarDinheiro(valor) {
    // Remove tudo que não é número
    valor = valor.replace(/\D/g, '');
    
    // Se não há valor, retorna vazio
    if (!valor) return '';
    
    // Converte para centavos
    valor = (parseInt(valor) / 100).toFixed(2);
    
    // Formata para padrão brasileiro
    valor = valor.replace('.', ',');
    valor = valor.replace(/(\d)(?=(\d{3})+(?!\d))/g, '$1.');
    
    return 'R$ ' + valor;
}

// Função para converter valor formatado para decimal (para enviar ao servidor)
function converterDinheiroParaDecimal(valorFormatado) {
    if (!valorFormatado) return 0;
    
    return parseFloat(
        valorFormatado
            .replace('R$', '')
            .replace(/\./g, '')
            .replace(',', '.')
            .trim()
    ) || 0;
}

// ================================
// ?? FORMATAÇÃO DE CPF
// ================================
function aplicarMascaraCpf(campo) {
    campo.addEventListener('input', function(e) {
        let valor = e.target.value.replace(/\D/g, '');
        
        if (valor.length <= 11) {
            valor = valor.replace(/(\d{3})(\d)/, '$1.$2');
            valor = valor.replace(/(\d{3})(\d)/, '$1.$2');
            valor = valor.replace(/(\d{3})(\d{1,2})$/, '$1-$2');
        }
        
        e.target.value = valor;
    });
}

// ================================
// ?? FORMATAÇÃO DE CNPJ
// ================================
function aplicarMascaraCnpj(campo) {
    campo.addEventListener('input', function(e) {
        let valor = e.target.value.replace(/\D/g, '');
        
        if (valor.length <= 14) {
            valor = valor.replace(/(\d{2})(\d)/, '$1.$2');
            valor = valor.replace(/(\d{3})(\d)/, '$1.$2');
            valor = valor.replace(/(\d{3})(\d)/, '$1/$2');
            valor = valor.replace(/(\d{4})(\d{1,2})$/, '$1-$2');
        }
        
        e.target.value = valor;
    });
}

// ================================
// ?? FORMATAÇÃO DE TELEFONE
// ================================
function aplicarMascaraTelefone(campo) {
    campo.addEventListener('input', function(e) {
        let valor = e.target.value.replace(/\D/g, '');
        
        if (valor.length <= 11) {
            if (valor.length <= 10) {
                // Telefone fixo: (XX) XXXX-XXXX
                valor = valor.replace(/(\d{2})(\d)/, '($1) $2');
                valor = valor.replace(/(\d{4})(\d)/, '$1-$2');
            } else {
                // Celular: (XX) XXXXX-XXXX
                valor = valor.replace(/(\d{2})(\d)/, '($1) $2');
                valor = valor.replace(/(\d{5})(\d)/, '$1-$2');
            }
        }
        
        e.target.value = valor;
    });
}

// ================================
// ?? FUNÇÕES AUXILIARES
// ================================

// Função para aplicar máscara manualmente
function aplicarMascara(campo, tipo) {
    switch(tipo.toLowerCase()) {
        case 'hora':
            aplicarMascaraHora(campo);
            break;
        case 'dinheiro':
            aplicarMascaraDinheiro(campo);
            break;
        case 'cpf':
            aplicarMascaraCpf(campo);
            break;
        case 'cnpj':
            aplicarMascaraCnpj(campo);
            break;
        case 'telefone':
            aplicarMascaraTelefone(campo);
            break;
    }
}

// Função para validar hora
function validarHora(hora) {
    const regex = /^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$/;
    return regex.test(hora);
}

// Função para validar CPF (algoritmo básico)
function validarCpf(cpf) {
    cpf = cpf.replace(/\D/g, '');
    
    if (cpf.length !== 11 || /^(\d)\1{10}$/.test(cpf)) {
        return false;
    }
    
    let soma = 0;
    for (let i = 0; i < 9; i++) {
        soma += parseInt(cpf.charAt(i)) * (10 - i);
    }
    
    let resto = 11 - (soma % 11);
    if (resto === 10 || resto === 11) resto = 0;
    if (resto !== parseInt(cpf.charAt(9))) return false;
    
    soma = 0;
    for (let i = 0; i < 10; i++) {
        soma += parseInt(cpf.charAt(i)) * (11 - i);
    }
    
    resto = 11 - (soma % 11);
    if (resto === 10 || resto === 11) resto = 0;
    
    return resto === parseInt(cpf.charAt(10));
}

// Função para validar CNPJ (algoritmo básico)
function validarCnpj(cnpj) {
    cnpj = cnpj.replace(/\D/g, '');
    
    if (cnpj.length !== 14) return false;
    
    // Validação básica (pode ser expandida)
    const tamanho = cnpj.length - 2;
    const numeros = cnpj.substring(0, tamanho);
    const digitos = cnpj.substring(tamanho);
    let soma = 0;
    let pos = tamanho - 7;
    
    for (let i = tamanho; i >= 1; i--) {
        soma += numeros.charAt(tamanho - i) * pos--;
        if (pos < 2) pos = 9;
    }
    
    let resultado = soma % 11 < 2 ? 0 : 11 - soma % 11;
    if (resultado !== parseInt(digitos.charAt(0))) return false;
    
    tamanho = tamanho + 1;
    numeros = cnpj.substring(0, tamanho);
    soma = 0;
    pos = tamanho - 7;
    
    for (let i = tamanho; i >= 1; i--) {
        soma += numeros.charAt(tamanho - i) * pos--;
        if (pos < 2) pos = 9;
    }
    
    resultado = soma % 11 < 2 ? 0 : 11 - soma % 11;
    
    return resultado === parseInt(digitos.charAt(1));
}

// ================================
// ?? CONVERSÕES PARA ENVIO
// ================================

// Preparar formulário antes do envio
function prepararFormularioParaEnvio(formElement) {
    const camposDinheiro = formElement.querySelectorAll('.dinheiro-input, input[name*="Custo"], input[name*="Preco"], input[name*="Valor"]');
    
    camposDinheiro.forEach(campo => {
        // Criar campo hidden com valor decimal
        const valorDecimal = converterDinheiroParaDecimal(campo.value);
        const hiddenInput = document.createElement('input');
        hiddenInput.type = 'hidden';
        hiddenInput.name = campo.name;
        hiddenInput.value = valorDecimal;
        
        // Renomear campo original para não ser enviado
        campo.name = campo.name + '_formatted';
        
        // Adicionar campo hidden ao formulário
        formElement.appendChild(hiddenInput);
    });
}

// Aplicar preparação automática em formulários
document.addEventListener('DOMContentLoaded', function() {
    const formularios = document.querySelectorAll('form');
    
    formularios.forEach(form => {
        form.addEventListener('submit', function(e) {
            prepararFormularioParaEnvio(this);
        });
    });
});

// ================================
// ?? EXEMPLO DE USO
// ================================

/*
<!-- HTML -->
<input type="text" class="dinheiro-input" name="CustoEstimado" placeholder="R$ 0,00" />
<input type="text" class="hora-input" name="HoraInicio" placeholder="00:00" />
<input type="text" class="cpf-input" name="Cpf" placeholder="000.000.000-00" />

<!-- JavaScript -->
<script src="~/js/formatacao.js"></script>
*/