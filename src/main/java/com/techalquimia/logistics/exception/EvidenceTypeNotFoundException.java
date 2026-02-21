package com.techalquimia.logistics.exception;

public class EvidenceTypeNotFoundException extends RuntimeException {

    public EvidenceTypeNotFoundException(String code) {
        super("Tipo de evidencia no encontrado: " + code);
    }
}
