package com.techalquimia.logistics.service;

import com.google.cloud.vision.v1.*;
import com.google.protobuf.ByteString;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Service;
import org.springframework.web.multipart.MultipartFile;

import java.io.IOException;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

/**
 * Extrae datos de tickets (peso total, tara, peso neto) mediante OCR.
 * Fórmula: peso total = tara + peso neto
 */
@Service
@Slf4j
public class TicketExtractionService {

    private static final Pattern PESO_TOTAL_PATTERN = Pattern.compile(
            "peso\\s*total|total\\s*peso|bruto", Pattern.CASE_INSENSITIVE);
    private static final Pattern TARA_PATTERN = Pattern.compile(
            "tara", Pattern.CASE_INSENSITIVE);
    private static final Pattern PESO_NETO_PATTERN = Pattern.compile(
            "peso\\s*neto|neto|peso", Pattern.CASE_INSENSITIVE);
    private static final Pattern NUMERIC_PATTERN = Pattern.compile("([0-9]+[.,][0-9]+|[0-9]+)");

    public record TicketData(Double pesoTotal, Double tara, Double pesoNeto) {}

    public TicketData extractFromImage(MultipartFile image) throws IOException {
        try (ImageAnnotatorClient visionClient = ImageAnnotatorClient.create()) {
            ByteString imgBytes = ByteString.copyFrom(image.getBytes());
            Image img = Image.newBuilder().setContent(imgBytes).build();
            Feature feature = Feature.newBuilder().setType(Feature.Type.TEXT_DETECTION).build();
            AnnotateImageRequest request = AnnotateImageRequest.newBuilder()
                    .addFeatures(feature)
                    .setImage(img)
                    .build();

            BatchAnnotateImagesResponse response = visionClient.batchAnnotateImages(
                    java.util.List.of(request));
            AnnotateImageResponse annotateResponse = response.getResponses(0);

            if (annotateResponse.hasError()) {
                log.warn("Error en OCR: {}", annotateResponse.getError().getMessage());
                return new TicketData(null, null, null);
            }

            String fullText = annotateResponse.getFullTextAnnotation().getText();
            return parseTicketData(fullText);
        } catch (IOException | RuntimeException e) {
            log.warn("OCR no disponible (credenciales GCP no configuradas o error): {}", e.getMessage());
            return new TicketData(null, null, null);
        }
    }

    private TicketData parseTicketData(String text) {
        Double pesoTotal = extractValueNearKeyword(text, PESO_TOTAL_PATTERN);
        Double tara = extractValueNearKeyword(text, TARA_PATTERN);
        Double pesoNeto = extractValueNearKeyword(text, PESO_NETO_PATTERN);

        // Validar: peso total ≈ tara + peso neto (si tenemos los 3)
        if (pesoTotal != null && tara != null && pesoNeto != null) {
            double sum = tara + pesoNeto;
            if (Math.abs(pesoTotal - sum) > 0.1) {
                log.debug("Ajuste: pesoTotal={}, tara={}, pesoNeto={} -> recalculando", pesoTotal, tara, pesoNeto);
                pesoTotal = sum;
            }
        } else if (pesoTotal != null && tara != null) {
            pesoNeto = pesoTotal - tara;
        } else if (pesoTotal != null && pesoNeto != null) {
            tara = pesoTotal - pesoNeto;
        } else if (tara != null && pesoNeto != null) {
            pesoTotal = tara + pesoNeto;
        }

        return new TicketData(pesoTotal, tara, pesoNeto);
    }

    private Double extractValueNearKeyword(String text, Pattern keywordPattern) {
        Matcher keywordMatcher = keywordPattern.matcher(text);
        while (keywordMatcher.find()) {
            int start = keywordMatcher.start();
            int end = Math.min(start + 50, text.length());
            String snippet = text.substring(Math.max(0, start - 10), end);
            Matcher numMatcher = NUMERIC_PATTERN.matcher(snippet);
            if (numMatcher.find()) {
                try {
                    String numStr = numMatcher.group(1).replace(',', '.');
                    return Double.parseDouble(numStr);
                } catch (NumberFormatException ignored) {
                }
            }
        }
        return null;
    }
}
